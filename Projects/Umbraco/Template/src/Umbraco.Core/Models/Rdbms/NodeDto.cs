﻿using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoNode")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class NodeDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_structure", IdentitySeed = 1045)]
        public int NodeId { get; set; }

        [Column("trashed")]
        [Constraint(Default = "0")]
        public bool Trashed { get; set; }

        [Column("parentID")]
        [ForeignKey(typeof(NodeDto))]
        [IndexAttribute(IndexTypes.NonClustered, Name = "IX_umbracoNodeParentId")]
        public int ParentId { get; set; }

        [Column("nodeUser")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? UserId { get; set; }

        [Column("level")]
        public short Level { get; set; }

        [Column("path")]
        [Length(150)]
        public string Path { get; set; }

        [Column("sortOrder")]
        public int SortOrder { get; set; }

        [Column("uniqueID")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public Guid? UniqueId { get; set; }

        [Column("text")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Text { get; set; }

        [Column("nodeObjectType")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [IndexAttribute(IndexTypes.NonClustered, Name = "IX_umbracoNodeObjectType")]
        public Guid? NodeObjectType { get; set; }

        [Column("createDate")]
        [Constraint(Default = "getdate()")]
        public DateTime CreateDate { get; set; }
    }
}