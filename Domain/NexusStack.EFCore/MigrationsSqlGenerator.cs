using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.EFCore
{
    public class MigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
    {
#pragma warning disable EF1001 // Internal EF Core API usage.
        public MigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, INpgsqlSingletonOptions npgsqlOptions)
#pragma warning restore EF1001 // Internal EF Core API usage.
            : base(dependencies, npgsqlOptions)
        {
        }

        protected override void Generate(CreateTableOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate = true)
        {
            operation.ForeignKeys.RemoveAll(a => true);
            base.Generate(operation, model, builder, terminate);
        }

        protected override void Generate(AlterTableOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            // AlertTable 取消外键

            base.Generate(operation, model, builder);
        }
    }
}
