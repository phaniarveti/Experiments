﻿using System;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute
{
    public class ExecuteBuilder : IExecuteBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseProviders[] _databaseProviders;

        public ExecuteBuilder(IMigrationContext context, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _databaseProviders = databaseProviders;
        }

        public void Sql(string sqlStatement)
        {
            var expression = _databaseProviders == null
                                 ? new ExecuteSqlStatementExpression {SqlStatement = sqlStatement}
                                 : new ExecuteSqlStatementExpression(_context.CurrentDatabaseProvider,
                                                                     _databaseProviders) {SqlStatement = sqlStatement};
            _context.Expressions.Add(expression);
        }

        public void Code(Func<Database, string> codeStatement)
        {
            var expression = _databaseProviders == null
                                 ? new ExecuteCodeStatementExpression { CodeStatement = codeStatement }
                                 : new ExecuteCodeStatementExpression(_context.CurrentDatabaseProvider,
                                                                     _databaseProviders) { CodeStatement = codeStatement };
            _context.Expressions.Add(expression);
        }
    }
}