﻿using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions
{
    public class AlterColumnExpression : MigrationExpressionBase
    {
        public AlterColumnExpression()
        {
            Column = new ColumnDefinition() { ModificationType = ModificationType.Alter };
        }

        public AlterColumnExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders) : base(current, databaseProviders)
        {
            Column = new ColumnDefinition() { ModificationType = ModificationType.Alter };
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual ColumnDefinition Column { get; set; }

        public override string ToString()
        {
            return string.Format(SqlSyntaxContext.SqlSyntaxProvider.AlterColumn,
                                 SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(TableName),
                                 SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName(Column.Name));
        }
    }
}