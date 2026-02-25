using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.FileStroage
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 读取配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddAliyunOSS(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AliyunOSSOption>(configuration.GetSection("AliOSS"));
            return services;
        }
    }
}
