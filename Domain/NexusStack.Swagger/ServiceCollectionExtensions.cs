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
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
using Unchase.Swashbuckle.AspNetCore.Extensions.Options;

namespace NexusStack.Swagger
{
    public static partial class ServiceCollectionExtensions
    {
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

                var action = new Action<FixEnumsOptions>(o =>
                {
                    o.IncludeDescriptions = true;
                    o.IncludeXEnumRemarks = true;
                    o.DescriptionSource = DescriptionSources.DescriptionAttributesThenXmlComments;
                });
                options.AddEnumsWithValuesFixFilters(action);

                // 加载程序运行目录下的所有 xml 注释文档
                Directory.GetFiles(AppContext.BaseDirectory, "*.xml").ToList().ForEach(comment => options.IncludeXmlComments(comment, true));

                options.OperationFilter<HttpHeaderFilter>(Array.Empty<object>());
            });

            return services;
        }


        public static IApplicationBuilder UseSwagger(this WebApplication app, string name, string title, string routePrefix = "docs", string documentTilte = "接口文档")
        {
            var swaggerOptions = app.Services.GetService<IOptions<SwaggerOptions>>();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.Equals("/swagger"))
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
                options.HeadContent = string.Empty;

                options.Interceptors.RequestInterceptorFunction = "function(request){return dvs.auth.requestInterceptor(request);}";

                if (app.Environment.IsDevelopment() || swaggerOptions.Value.Endpoints == null)
                {
                    options.SwaggerEndpoint($"/api/{name}/swagger.json", title);
                }
                else
                {
                    swaggerOptions.Value?.Endpoints?.ForEach(a => options.SwaggerEndpoint(a.Url, a.Name));
                }

                options.InjectJavascript("/docs/static/dvs-swagger.js");
                options.InjectStylesheet("/docs/static/dvs-swagger.css");

                options.HeadContent = $"<script type='text/javascript'>var dvs = dvs || {{}};dvs.host='{commonOptions!.Value.Host}'?'{commonOptions.Value.Host}':location.origin;</script>";

                //此处需要将文件设置为嵌入的资源
                options.IndexStream = () => Assembly.GetExecutingAssembly().GetManifestResourceStream("NexusStack.Swagger.Resources.dvs-swagger.html");
            });

            app.MapGet("/docs/static/dvs-swagger.js", async () =>
            {
                var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

                var file = fileProvider.GetFileInfo("Resources/dvs-swagger.js");

                if (file.Exists)
                {
                    using var stream = file.CreateReadStream();

                    var bytes = new byte[stream.Length];
                    await stream.ReadAsync(bytes);

                    return Results.File(bytes, contentType: "application/javascript");
                }
                return Results.NotFound();
            });

            app.MapGet("/docs/static/dvs-swagger.css", async () =>
            {
                var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

                var file = fileProvider.GetFileInfo("Resources/dvs-swagger.css");

                if (file.Exists)
                {
                    using var stream = file.CreateReadStream();

                    var bytes = new byte[stream.Length];
                    await stream.ReadAsync(bytes);

                    return Results.File(bytes, contentType: "text/css");
                }
                return Results.NotFound();
            });

            return app;
        }
    }
}
