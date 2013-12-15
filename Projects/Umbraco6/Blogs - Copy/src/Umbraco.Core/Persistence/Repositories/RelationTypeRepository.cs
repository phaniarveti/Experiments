﻿using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="RelationType"/>
    /// </summary>
    internal class RelationTypeRepository : PetaPocoRepositoryBase<int, RelationType>, IRelationTypeRepository
    {
		public RelationTypeRepository(IDatabaseUnitOfWork work)
			: base(work)
        {
        }

		public RelationTypeRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache)
            : base(work, cache)
        {
        }

        #region Overrides of RepositoryBase<int,RelationType>

        protected override RelationType PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var dto = Database.FirstOrDefault<RelationTypeDto>(sql);
            if (dto == null)
                return null;

            var factory = new RelationTypeFactory();
            var entity = factory.BuildEntity(dto);

            entity.ResetDirtyProperties();

            return entity;
        }

        protected override IEnumerable<RelationType> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var dtos = Database.Fetch<RelationTypeDto>("WHERE id > 0");
                foreach (var dto in dtos)
                {
                    yield return Get(dto.Id);
                }
            }
        }

        protected override IEnumerable<RelationType> PerformGetByQuery(IQuery<RelationType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<RelationType>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<RelationTypeDto>(sql);

            foreach (var dto in dtos)
            {
                yield return Get(dto.Id);
            }
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,RelationType>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
               .From<RelationTypeDto>();
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoRelationType.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               "DELETE FROM umbracoRelation WHERE relType = @Id",
                               "DELETE FROM umbracoRelationType WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(RelationType entity)
        {
            entity.AddingEntity();

            var factory = new RelationTypeFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(RelationType entity)
        {
            entity.UpdatingEntity();

            var factory = new RelationTypeFactory();
            var dto = factory.BuildDto(entity);
            Database.Update(dto);

            entity.ResetDirtyProperties();
        }

        #endregion
    }
}