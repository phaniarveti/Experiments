﻿using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class DataTypeDefinitionMapperTest
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = DataTypeDefinitionMapper.Instance.Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[id]"));
        }

        [Test]
        public void Can_Map_Key_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = DataTypeDefinitionMapper.Instance.Map("Key");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[uniqueID]"));
        }

        [Test]
        public void Can_Map_DatabaseType_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = DataTypeDefinitionMapper.Instance.Map("DatabaseType");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDataType].[dbType]"));
        }

        [Test]
        public void Can_Map_ControlId_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = DataTypeDefinitionMapper.Instance.Map("ControlId");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDataType].[controlId]"));
        }
    }
}