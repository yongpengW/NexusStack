using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.EFCore
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 初始化 PostgreSQL 配置，将 EFCore、PosPgSqlContext 注入到容器中
        /// </summary>
        public static IServiceCollection AddEFCoreAndPostgreSQL(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. 从 AgileConfig 获取连接字符串，假设 Key 为 "PostgreSQL"
            var connectionString = configuration.GetConnectionString("PostgreSQL");

            services.AddTransient<MainSaveChangeInterceptor>();

            // 2. 注册 DbContext
            services.AddDbContextPool<MainContext>((sp, options) =>
            {
                // 注册自定义拦截器
                options.AddInterceptors(sp.GetRequiredService<MainSaveChangeInterceptor>());

                // 移除外键（保持与原有逻辑一致）
                options.UseRemoveForeignKeys();

                // 禁止跟踪
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                // 3. 使用 Npgsql
                options.UseNpgsql(connectionString, pgOptions =>
                {
                    pgOptions.MigrationsAssembly("NexusStack.WebAPI");

                    pgOptions.EnableRetryOnFailure();

                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                })
                .EnableSensitiveDataLogging(false)
                .EnableDetailedErrors(false);
            }, poolSize: 1024);

            // 注册仓储服务（保持不变）
            services.AddScoped(typeof(IServiceBase<,>), typeof(ServiceBase<,>));
            services.AddScoped(typeof(IServiceBase<>), typeof(ServiceBase<>));

            return services;
        }

        /// <summary>
        /// 移除表结构中的外键约束
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static DbContextOptionsBuilder UseRemoveForeignKeys(this DbContextOptionsBuilder builder)
        {
            builder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
            return builder;
        }
    }
}
