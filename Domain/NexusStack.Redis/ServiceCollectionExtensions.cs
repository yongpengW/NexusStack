using CSRedis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Redis
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 初始化Redis配置
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRedis(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.AddRedis(options =>
            {
                options.ConnectionString = configuration.GetSection("Redis:ConnectionString").Value!;
            });

            return app;
        }

        /// <summary>
        /// 初始化Redis配置
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private static IApplicationBuilder AddRedis(this IApplicationBuilder app, Action<RedisOption> configure)
        {
            RedisOption options = new();
            configure(options);
            string redisConnectionString = options.ConnectionString;
            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                throw new Exception("Redis连接字符串不能为空");
            }
            var csredis = new CSRedisClient(redisConnectionString);
            RedisHelper.Initialization(csredis);
            return app;
        }
    }
}
