﻿using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsTags")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class TagDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("tag")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(200)]
        public string Tag { get; set; }//NOTE Is set to [varchar] (200) in Sql Server script

        [Column("ParentId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? ParentId { get; set; }

        [Column("group")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(100)]
        public string Group { get; set; }//NOTE Is set to [varchar] (100) in Sql Server script
    }
}