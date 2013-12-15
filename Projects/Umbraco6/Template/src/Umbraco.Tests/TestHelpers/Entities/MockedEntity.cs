﻿using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Tests.TestHelpers.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class MockedEntity : Entity
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}