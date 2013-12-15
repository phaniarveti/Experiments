using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Initial
{
    /// <summary>
    /// Represents the initial database schema creation by running CreateTable for all DTOs against the db.
    /// </summary>
    internal class DatabaseSchemaCreation
    {
        #region Private Members
        private readonly Database _database;
        private static readonly Dictionary<int, Type> OrderedTables = new Dictionary<int, Type>
                                                                          {
                                                                              {0, typeof (NodeDto)},
                                                                              {1, typeof (TemplateDto)},
                                                                              {2, typeof (ContentDto)},
                                                                              {3, typeof (ContentVersionDto)},
                                                                              {4, typeof (DocumentDto)},
                                                                              {5, typeof (ContentTypeDto)},
                                                                              {6, typeof (DocumentTypeDto)},
                                                                              {7, typeof (DataTypeDto)},
                                                                              {8, typeof (DataTypePreValueDto)},
                                                                              {9, typeof (DictionaryDto)},
                                                                              {10, typeof (LanguageTextDto)},
                                                                              {11, typeof (LanguageDto)},
                                                                              {12, typeof (DomainDto)},
                                                                              {13, typeof (LogDto)},
                                                                              {14, typeof (MacroDto)},
                                                                              {15, typeof (MacroPropertyDto)},
                                                                              {16, typeof (MemberTypeDto)},
                                                                              {17, typeof (MemberDto)},
                                                                              {18, typeof (Member2MemberGroupDto)},
                                                                              {19, typeof (ContentXmlDto)},
                                                                              {20, typeof (PreviewXmlDto)},
                                                                              {21, typeof (PropertyTypeGroupDto)},
                                                                              {22, typeof (PropertyTypeDto)},
                                                                              {23, typeof (PropertyDataDto)},
                                                                              {24, typeof (RelationTypeDto)},
                                                                              {25, typeof (RelationDto)},
                                                                              {26, typeof (StylesheetDto)},
                                                                              {27, typeof (StylesheetPropertyDto)},
                                                                              {28, typeof (TagDto)},
                                                                              {29, typeof (TagRelationshipDto)},
                                                                              {30, typeof (UserLoginDto)},
                                                                              {31, typeof (UserTypeDto)},
                                                                              {32, typeof (UserDto)},
                                                                              {33, typeof (TaskTypeDto)},
                                                                              {34, typeof (TaskDto)},
                                                                              {35, typeof (ContentType2ContentTypeDto)},
                                                                              {36, typeof (ContentTypeAllowedContentTypeDto)},
                                                                              {37, typeof (User2AppDto)},
                                                                              {38, typeof (User2NodeNotifyDto)},
                                                                              {39, typeof (User2NodePermissionDto)},
                                                                              {40, typeof (ServerRegistrationDto)}
                                                                          };
        #endregion
        
        /// <summary>
        /// Drops all Umbraco tables in the db
        /// </summary>
        internal void UninstallDatabaseSchema()
        {
            LogHelper.Info<DatabaseSchemaCreation>("Start UninstallDatabaseSchema");

            foreach (var item in OrderedTables.OrderByDescending(x => x.Key))
            {
                var tableNameAttribute = item.Value.FirstAttribute<TableNameAttribute>();

                string tableName = tableNameAttribute == null ? item.Value.Name : tableNameAttribute.Value;

                LogHelper.Info<DatabaseSchemaCreation>("Uninstall" + tableName);

                try
                {
                    _database.DropTable(tableName);
                }
                catch (Exception ex)
                {
                    //swallow this for now, not sure how best to handle this with diff databases... though this is internal
                    // and only used for unit tests. If this fails its because the table doesn't exist... generally!
                    LogHelper.Error<DatabaseSchemaCreation>("Could not drop table " + tableName, ex);
                }
            }
        }

        public DatabaseSchemaCreation(Database database)
        {
            _database = database;
        }

        /// <summary>
        /// Initialize the database by creating the umbraco db schema
        /// </summary>
        public void InitializeDatabaseSchema()
        {
            var e = new DatabaseCreationEventArgs();
            FireBeforeCreation(e);

            if (!e.Cancel)
            {
                foreach (var item in OrderedTables.OrderBy(x => x.Key))
                {
                    _database.CreateTable(false, item.Value);
                }
            }

            FireAfterCreation(e);
        }

        /// <summary>
        /// Validates the schema of the current database
        /// </summary>
        public DatabaseSchemaResult ValidateSchema()
        {
            var result = new DatabaseSchemaResult();

            foreach (var item in OrderedTables.OrderBy(x => x.Key))
            {
                var tableDefinition = DefinitionFactory.GetTableDefinition(item.Value);
                result.TableDefinitions.Add(tableDefinition);
            }

            //Check tables in configured database against tables in schema
            var tablesInDatabase = SqlSyntaxContext.SqlSyntaxProvider.GetTablesInSchema(_database).ToList();
            var tablesInSchema = result.TableDefinitions.Select(x => x.Name).ToList();
            //Add valid and invalid table differences to the result object
            var validTableDifferences = tablesInDatabase.Intersect(tablesInSchema);
            foreach (var tableName in validTableDifferences)
            {
                result.ValidTables.Add(tableName);
            }
            var invalidTableDifferences =
                tablesInDatabase.Except(tablesInSchema)
                                .Union(tablesInSchema.Except(tablesInDatabase));
            foreach (var tableName in invalidTableDifferences)
            {
                result.Errors.Add(new Tuple<string, string>("Table", tableName));
            }

            //Check columns in configured database against columns in schema
            var columnsInDatabase = SqlSyntaxContext.SqlSyntaxProvider.GetColumnsInSchema(_database);
            var columnsPerTableInDatabase = columnsInDatabase.Select(x => string.Concat(x.TableName, ",", x.ColumnName)).ToList();
            var columnsPerTableInSchema = result.TableDefinitions.SelectMany(x => x.Columns.Select(y => string.Concat(y.TableName, ",", y.Name))).ToList();
            //Add valid and invalid column differences to the result object
            var validColumnDifferences = columnsPerTableInDatabase.Intersect(columnsPerTableInSchema);
            foreach (var column in validColumnDifferences)
            {
                result.ValidColumns.Add(column);
            }
            var invalidColumnDifferences = columnsPerTableInDatabase.Except(columnsPerTableInSchema);
            foreach (var column in invalidColumnDifferences)
            {
                result.Errors.Add(new Tuple<string, string>("Column", column));
            }

            //MySql doesn't conform to the "normal" naming of constraints, so there is currently no point in doing these checks.
            //NOTE: At a later point we do other checks for MySql, but ideally it should be necessary to do special checks for different providers.
            if (SqlSyntaxContext.SqlSyntaxProvider is MySqlSyntaxProvider)
                return result;

            //Check constraints in configured database against constraints in schema
            var constraintsInDatabase = SqlSyntaxContext.SqlSyntaxProvider.GetConstraintsPerColumn(_database).DistinctBy(x => x.Item3).ToList();
            var foreignKeysInDatabase = constraintsInDatabase.Where(x => x.Item3.StartsWith("FK_")).Select(x => x.Item3).ToList();
            var primaryKeysInDatabase = constraintsInDatabase.Where(x => x.Item3.StartsWith("PK_")).Select(x => x.Item3).ToList();
            var indexesInDatabase = constraintsInDatabase.Where(x => x.Item3.StartsWith("IX_")).Select(x => x.Item3).ToList();
            var unknownConstraintsInDatabase =
                constraintsInDatabase.Where(
                    x =>
                    x.Item3.StartsWith("FK_") == false && x.Item3.StartsWith("PK_") == false &&
                    x.Item3.StartsWith("IX_") == false).Select(x => x.Item3).ToList();
            var foreignKeysInSchema = result.TableDefinitions.SelectMany(x => x.ForeignKeys.Select(y => y.Name)).ToList();
            var primaryKeysInSchema = result.TableDefinitions.SelectMany(x => x.Columns.Select(y => y.PrimaryKeyName)).ToList();
            var indexesInSchema = result.TableDefinitions.SelectMany(x => x.Indexes.Select(y => y.Name)).ToList();
            //Add valid and invalid foreign key differences to the result object
            foreach (var unknown in unknownConstraintsInDatabase)
            {
                if (foreignKeysInSchema.Contains(unknown) || primaryKeysInSchema.Contains(unknown) || indexesInSchema.Contains(unknown))
                {
                    result.ValidConstraints.Add(unknown);
                }
                else
                {
                    result.Errors.Add(new Tuple<string, string>("Unknown", unknown));
                }
            }
            var validForeignKeyDifferences = foreignKeysInDatabase.Intersect(foreignKeysInSchema);
            foreach (var foreignKey in validForeignKeyDifferences)
            {
                result.ValidConstraints.Add(foreignKey);
            }
            var invalidForeignKeyDifferences = foreignKeysInDatabase.Except(foreignKeysInSchema);
            foreach (var foreignKey in invalidForeignKeyDifferences)
            {
                result.Errors.Add(new Tuple<string, string>("Constraint", foreignKey));
            }
            //Add valid and invalid primary key differences to the result object
            var validPrimaryKeyDifferences = primaryKeysInDatabase.Intersect(primaryKeysInSchema);
            foreach (var primaryKey in validPrimaryKeyDifferences)
            {
                result.ValidConstraints.Add(primaryKey);
            }
            var invalidPrimaryKeyDifferences = primaryKeysInDatabase.Except(primaryKeysInSchema);
            foreach (var primaryKey in invalidPrimaryKeyDifferences)
            {
                result.Errors.Add(new Tuple<string, string>("Constraint", primaryKey));
            }
            //Add valid and invalid index differences to the result object
            var validIndexDifferences = indexesInDatabase.Intersect(indexesInSchema);
            foreach (var index in validIndexDifferences)
            {
                result.ValidConstraints.Add(index);
            }
            var invalidIndexDifferences = indexesInDatabase.Except(indexesInSchema);
            foreach (var index in invalidIndexDifferences)
            {
                result.Errors.Add(new Tuple<string, string>("Constraint", index));
            }

            return result;
        }

        #region Events

        /// <summary>
        /// The save event handler
        /// </summary>
        internal delegate void DatabaseEventHandler(DatabaseCreationEventArgs e);

        /// <summary>
        /// Occurs when [before save].
        /// </summary>
        internal static event DatabaseEventHandler BeforeCreation;
        /// <summary>
        /// Raises the <see cref="BeforeCreation"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected internal virtual void FireBeforeCreation(DatabaseCreationEventArgs e)
        {
            if (BeforeCreation != null)
            {
                BeforeCreation(e);
            }
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        internal static event DatabaseEventHandler AfterCreation;
        /// <summary>
        /// Raises the <see cref="AfterCreation"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterCreation(DatabaseCreationEventArgs e)
        {
            if (AfterCreation != null)
            {
                AfterCreation(e);
            }
        }

        #endregion
    }
}