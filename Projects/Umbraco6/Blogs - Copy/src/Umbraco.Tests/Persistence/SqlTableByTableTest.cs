﻿using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture, NUnit.Framework.Ignore]
    public class SqlTableByTableTest : BaseTableByTableTest
    {
        private Database _database;

        #region Overrides of BaseTableByTableTest

        [SetUp]
        public override void Initialize()
        {
            TestHelper.SetupLog4NetForTests();
            TestHelper.InitializeContentDirectories();

            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            RepositoryResolver.Current = new RepositoryResolver(
                new RepositoryFactory());

            Resolution.Freeze();
            ApplicationContext.Current = new ApplicationContext(
                //assign the db context
                new DatabaseContext(new DefaultDatabaseFactory()),
                //assign the service context
                new ServiceContext(new PetaPocoUnitOfWorkProvider(), new FileUnitOfWorkProvider(), new PublishingStrategy())) { IsReady = true };

            SqlSyntaxContext.SqlSyntaxProvider = SqlServerSyntax.Provider;

            _database = new Database(@"server=.\SQLEXPRESS;database=EmptyForTest;user id=umbraco;password=umbraco",
                                     "System.Data.SqlClient");
        }

        [TearDown]
        public override void TearDown()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", null);

            SqlSyntaxContext.SqlSyntaxProvider = null;

            //reset the app context
            ApplicationContext.Current = null;
            Resolution.IsFrozen = false;

            RepositoryResolver.Reset();
        }

        public override Database Database
        {
            get { return _database; }
        }

        #endregion
    }
}