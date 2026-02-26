using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using NexusStack.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NexusStack.Swagger
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 读取嵌入资源内容
        /// </summary>
        private static string ReadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                Console.WriteLine($"[Swagger] Warning: Embedded resource not found: {resourceName}");
                return string.Empty;
            }

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// 添加 Swagger 文档
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="title"></param>
        /// <param name="version"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static IServiceCollection AddSwaggerGen(this IServiceCollection services, string name, string title, string version = "v1", string description = "")
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(name, new OpenApiInfo { Title = title, Version = version, Description = description });

                options.AddSecurityDefinition("Token", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Description = "Token"
                });

                // 使用 Swashbuckle 内置的枚举描述支持
                options.UseInlineDefinitionsForEnums();
                
                // 加载 XML 注释文档
                Directory.GetFiles(AppContext.BaseDirectory, "*.xml").ToList()
                    .ForEach(comment => options.IncludeXmlComments(comment, true));

                options.OperationFilter<HttpHeaderFilter>(Array.Empty<object>());
            });

            return services;
        }


        public static IApplicationBuilder UseSwagger(this WebApplication app, string name, string title, string routePrefix = "docs", string documentTilte = "接口文档")
        {
            var swaggerOptions = app.Services.GetService<IOptions<SwaggerOptions>>();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value?.Equals("/swagger") == true)
                {
                    context.Response.Redirect("/docs/index.html");
                    return;
                }
                await next(context);
            });

            app.UseSwagger(options =>
            {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0;
                options.RouteTemplate = "api/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(options =>
            {
                var commonOptions = app.Services.GetService<IOptions<CommonOptions>>();

                options.RoutePrefix = routePrefix;
                options.DocumentTitle = documentTilte;

                if (app.Environment.IsDevelopment() || swaggerOptions?.Value.Endpoints == null)
                {
                    options.SwaggerEndpoint($"/api/{name}/swagger.json", title);
                }
                else
                {
                    swaggerOptions.Value?.Endpoints?.ForEach(a => options.SwaggerEndpoint(a.Url, a.Name));
                }

                // 读取嵌入的 JavaScript 和 CSS 内容
                var jsContent = ReadEmbeddedResource("NexusStack.Swagger.Resources.nexusstack-swagger.js");
                var cssContent = ReadEmbeddedResource("NexusStack.Swagger.Resources.nexusstack-swagger.css");

                // 直接在 HeadContent 中内联 JavaScript 和 CSS
                var safeHost = commonOptions!.Value.Host?.Replace("\\", "\\\\").Replace("'", "\\'") ?? string.Empty;
                var hostScript = $"var nexusstack = nexusstack || {{}};nexusstack.host='{safeHost}'||location.origin;";
                options.HeadContent = $"<script type='text/javascript'>{hostScript}{jsContent}</script><style>{cssContent}</style>";

                options.Interceptors.RequestInterceptorFunction = "function(request){if(window.nexusstack && nexusstack.auth && nexusstack.auth.requestInterceptor){return nexusstack.auth.requestInterceptor(request);}return request;}";
            });

            return app;
        }
    }
}
