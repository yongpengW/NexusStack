using Microsoft.Extensions.DependencyInjection;
using System;

namespace NexusStack.Infrastructure.Client
{
    public static class HttpClientServiceExtensions
    {
        /// <summary>
        /// 注册 HttpClient 服务
        /// </summary>
        public static IServiceCollection AddHttpRequestClient(this IServiceCollection services)
        {
            services.AddHttpClient<HttpClientService>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // 设置 HttpMessageHandler 生命周期为 5 分钟
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    MaxConnectionsPerServer = 100,
                    // 如果需要忽略 SSL 证书验证（仅开发环境使用）
                    // ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            return services;
        }

        /// <summary>
        /// 注册 HttpClient 服务（自定义配置）
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureHandler">自定义 HttpClientHandler 配置</param>
        /// <param name="handlerLifetime">Handler 生命周期，默认 5 分钟</param>
        public static IServiceCollection AddHttpRequestClient(
            this IServiceCollection services,
            Action<HttpClientHandler> configureHandler,
            TimeSpan? handlerLifetime = null)
        {
            services.AddHttpClient<HttpClientService>()
                .SetHandlerLifetime(handlerLifetime ?? TimeSpan.FromMinutes(5))
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler
                    {
                        AllowAutoRedirect = true,
                        MaxConnectionsPerServer = 100
                    };
                    configureHandler?.Invoke(handler);
                    return handler;
                });

            return services;
        }
    }
}
