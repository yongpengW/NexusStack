using AutoMapper;
using DynamicLocalizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.AspNetCore.StaticFiles; 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NexusStack.Core.Authentication;
using NexusStack.Core.Filters;
using NexusStack.Core.Gateway;
using NexusStack.Core.HostedServices;
using NexusStack.Core.Schedules;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Attributes;
using NexusStack.Infrastructure.Converters;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Options;
using NexusStack.Infrastructure.TypeFinders;
using NexusStack.Infrastructure.Utils;
using NexusStack.Swagger;
using NexusStack.Redis;
using NexusStack.EFCore;
using NexusStack.RabbitMQ;
using NexusStack.Serilog;
using NexusStack.Infrastructure.FileStroage;
using NexusStack.Infrastructure.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Yarp.ReverseProxy.Configuration;
using JsonLongConverter = NexusStack.Infrastructure.Converters.JsonLongConverter;
using Newtonsoft.Json.Linq;
using NexusStack.Core.Services.EventAlerts;
using AgileConfig.Client;
using AgileConfig.Client.RegisterCenter;
using NexusStack.Core.Middlewares;

namespace NexusStack.Core
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 项目初始化函数
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="moduleKey"></param>
        /// <param name="moduleTitle"></param>
        /// <param name="posServiceType">POS服务类型</param>
        /// <param name="enableSignalR">是否启用SignalR自动映射</param>
        /// <returns></returns>
        /// 
        public static async Task InitAppliation(this WebApplicationBuilder builder, string moduleKey, string moduleTitle, CoreServiceType coreServiceType = CoreServiceType.WebService, bool enableSignalR = false)
        {
            //ExcelPackage.LicenseContext = LicenseContext.Commercial;

            builder.AddBuilderServices(moduleKey, moduleTitle, coreServiceType);

            var app = builder.Build();

            app.UseApp(moduleKey, moduleTitle, coreServiceType);

            // 如果启用SignalR
            if (enableSignalR)
            {
                // Web API：自动映射Hub端点
                if (coreServiceType == CoreServiceType.WebService)
                {
                    AutoMapSignalRHubs(app);
                }
            }

            await app.RunAsync();
        }

        /// <summary>
        /// 自动映射所有带SignalRHub特性的Hub
        /// </summary>
        /// <param name="app"></param>
        private static void AutoMapSignalRHubs(WebApplication app)
        {
            try
            {
                Console.WriteLine("开始自动映射SignalR Hubs...");

                // 优先尝试使用配置文件中的HubPathMappings
                if (MapHubsFromConfiguration(app))
                {
                    Console.WriteLine("使用配置文件HubPathMappings映射Hub完成");
                    return;
                }

                // 如果配置文件映射失败，回退到反射方式
                Console.WriteLine("配置文件映射失败，回退到反射方式映射Hub...");
                MapHubsUsingReflection(app);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR Hub自动映射过程中发生错误: {ex.Message}");
                Console.WriteLine($"错误详情: {ex}");
            }
        }

        /// <summary>
        /// 基于配置文件HubPathMappings映射Hub
        /// </summary>
        /// <param name="app"></param>
        /// <returns>是否映射成功</returns>
        private static bool MapHubsFromConfiguration(WebApplication app)
        {
            try
            {
                // 获取SignalR通知选项配置
                var signalROptions = app.Services.GetService<IOptions<SignalRNotificationOptions>>()?.Value;
                if (signalROptions?.HubPathMappings == null || !signalROptions.HubPathMappings.Any())
                {
                    Console.WriteLine("未找到SignalR配置或HubPathMappings为空");
                    return false;
                }

                Console.WriteLine($"从配置文件加载到 {signalROptions.HubPathMappings.Count} 个Hub路径映射");

                int mappedHubsCount = 0;
                var hubTypeCache = new Dictionary<string, Type>();

                // 预加载所有Hub类型到缓存中
                PreloadHubTypes(hubTypeCache);

                foreach (var pathMapping in signalROptions.HubPathMappings)
                {
                    var hubPath = pathMapping.Key;
                    var hubNames = pathMapping.Value;

                    Console.WriteLine($"处理Hub路径: {hubPath} -> [{string.Join(", ", hubNames)}]");

                    // 对于每个路径，只映射第一个有效的Hub（主要Hub）
                    var primaryHubName = hubNames.FirstOrDefault();
                    if (string.IsNullOrEmpty(primaryHubName))
                    {
                        Console.WriteLine($"  ⚠️ 路径 {hubPath} 没有配置Hub名称");
                        continue;
                    }

                    if (hubTypeCache.TryGetValue(primaryHubName, out var hubType))
                    {
                        try
                        {
                            // 使用反射调用MapHub方法
                            var mapHubMethod = typeof(HubEndpointRouteBuilderExtensions)
                                .GetMethods()
                                .Where(m => m.Name == "MapHub" &&
                                           m.IsGenericMethodDefinition &&
                                           m.GetParameters().Length == 2 &&
                                           m.GetParameters()[1].ParameterType == typeof(string))
                                .FirstOrDefault();

                            if (mapHubMethod != null)
                            {
                                var genericMethod = mapHubMethod.MakeGenericMethod(hubType);
                                genericMethod.Invoke(null, new object[] { app, hubPath });

                                mappedHubsCount++;
                                Console.WriteLine($"  ✓ 成功映射Hub: {primaryHubName} -> {hubPath}");
                            }
                            else
                            {
                                Console.WriteLine($"  ✗ 找不到MapHub方法，无法映射Hub: {primaryHubName}");
                            }
                        }
                        catch (Exception hubMapEx)
                        {
                            Console.WriteLine($"  ✗ 映射Hub失败: {primaryHubName} -> {hubPath}, 错误: {hubMapEx.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"  ⚠️ 找不到Hub类型: {primaryHubName}，跳过路径 {hubPath}");
                    }
                }

                Console.WriteLine($"基于配置文件的SignalR Hub映射完成，共映射 {mappedHubsCount} 个Hub");
                return mappedHubsCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"基于配置文件映射Hub时发生错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 预加载所有Hub类型到缓存
        /// </summary>
        /// <param name="hubTypeCache"></param>
        private static void PreloadHubTypes(Dictionary<string, Type> hubTypeCache)
        {
            try
            {
                // 查找当前应用域中所有程序集
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetName().Name?.StartsWith("NexusStack.") == true);

                Console.WriteLine($"预加载Hub类型，检查 {assemblies.Count()} 个NexusStack相关程序集");

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        // 查找所有继承自Hub的类型
                        var hubTypes = assembly.GetTypes()
                            .Where(t => t.IsClass &&
                                       !t.IsAbstract &&
                                       typeof(Hub).IsAssignableFrom(t))
                            .ToList();

                        foreach (var hubType in hubTypes)
                        {
                            var hubName = hubType.Name;
                            if (!hubTypeCache.ContainsKey(hubName))
                            {
                                hubTypeCache[hubName] = hubType;
                                Console.WriteLine($"  缓存Hub类型: {hubName} ({hubType.FullName})");
                            }
                        }
                    }
                    catch (Exception assemblyEx)
                    {
                        Console.WriteLine($"预加载程序集 {assembly.GetName().Name} 的Hub类型时出错: {assemblyEx.Message}");
                    }
                }

                Console.WriteLine($"Hub类型预加载完成，共缓存 {hubTypeCache.Count} 个Hub类型");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"预加载Hub类型时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 使用反射方式映射Hub（回退方法）
        /// </summary>
        /// <param name="app"></param>
        private static void MapHubsUsingReflection(WebApplication app)
        {
            try
            {
                // 查找当前应用域中所有程序集
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetName().Name?.StartsWith("NexusStack.") == true);

                Console.WriteLine($"找到 {assemblies.Count()} 个NexusStack相关程序集");

                int mappedHubsCount = 0;

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        Console.WriteLine($"正在检查程序集: {assembly.GetName().Name}");

                        // 查找所有继承自Hub且带有SignalRHub特性的类型
                        var hubTypes = assembly.GetTypes()
                            .Where(t => t.IsClass &&
                                       !t.IsAbstract &&
                                       typeof(Hub).IsAssignableFrom(t) &&
                                       t.GetCustomAttributes(typeof(SignalRHubAttribute), false).Length > 0)
                            .ToList();

                        Console.WriteLine($"在程序集 {assembly.GetName().Name} 中找到 {hubTypes.Count} 个Hub类型");

                        foreach (var hubType in hubTypes)
                        {
                            var hubAttribute = (SignalRHubAttribute)hubType.GetCustomAttributes(typeof(SignalRHubAttribute), false).First();

                            Console.WriteLine($"正在映射Hub: {hubType.Name} -> {hubAttribute.Route}");

                            try
                            {
                                // 使用反射调用MapHub方法
                                var mapHubMethod = typeof(HubEndpointRouteBuilderExtensions)
                                    .GetMethods()
                                    .Where(m => m.Name == "MapHub" &&
                                               m.IsGenericMethodDefinition &&
                                               m.GetParameters().Length == 2 &&
                                               m.GetParameters()[1].ParameterType == typeof(string))
                                    .FirstOrDefault();

                                if (mapHubMethod != null)
                                {
                                    var genericMethod = mapHubMethod.MakeGenericMethod(hubType);
                                    genericMethod.Invoke(null, new object[] { app, hubAttribute.Route });

                                    mappedHubsCount++;
                                    Console.WriteLine($"✓ 成功映射Hub: {hubType.Name} -> {hubAttribute.Route}");
                                }
                                else
                                {
                                    Console.WriteLine($"✗ 找不到MapHub方法，无法映射Hub: {hubType.Name}");
                                }
                            }
                            catch (Exception hubMapEx)
                            {
                                Console.WriteLine($"✗ 映射Hub失败: {hubType.Name} -> {hubAttribute.Route}, 错误: {hubMapEx.Message}");
                            }
                        }
                    }
                    catch (Exception assemblyEx)
                    {
                        Console.WriteLine($"检查程序集 {assembly.GetName().Name} 时出错: {assemblyEx.Message}");
                    }
                }

                Console.WriteLine($"基于反射的SignalR Hub映射完成，共映射 {mappedHubsCount} 个Hub");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"基于反射映射Hub时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 应用程序启动时初始化
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="moduleKey"></param>
        /// <param name="moduleTitle"></param>
        /// <param name="posServiceType">POS服务类型</param>
        /// <returns></returns>
        public static WebApplicationBuilder AddBuilderServices(this WebApplicationBuilder builder, string moduleKey, string moduleTitle, CoreServiceType coreServiceType = CoreServiceType.WebService)
        {
            builder.Host.InitHostAndConfig(moduleKey, coreServiceType);
            builder.Services.AddDynamicLocalizer(new DynamicLocalizerOption()
            {
                LoadResource = () =>
                {
                    Dictionary<string, string> resource = new Dictionary<string, string>();
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    List<string> jsonFileNames = new List<string>() { "en.json", "zh.json" };

                    foreach (var jsonFileName in jsonFileNames)
                    {
                        String jsonfile = assembly.GetName().Name + $".Language.{jsonFileName}";
                        Stream fileStream = assembly.GetManifestResourceStream(jsonfile);

                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            var data = reader.ReadToEnd();
                            JObject jobject = JObject.Parse(data);
                            var culture = jobject["culture"];
                            foreach (JProperty property in jobject["texts"])
                            {
                                if (!resource.ContainsKey($"{property.Name}.{culture}"))
                                {
                                    resource.Add($"{property.Name}.{culture}", property.Value?.ToString());
                                }
                            }
                        }
                    }

                    return resource;
                },
                FormatCulture = (e => e.ToString().Replace("-", "_")),
                DefaultCulture = "en"
            });

            // 注册IHttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(moduleKey, moduleTitle);

            builder.Services.ConfigureOptions(builder.Configuration);

            // 注意: CommonSMSService、SMTPEmailService、EventAlertService 等实现了 IScopedDependency 的服务
            // 会通过后续的 AddServices<IScopedDependency> 自动注册，无需手动注册
            //builder.Services.AddScoped<OperationLogArchiveService>();

            builder.Services.AddHttpLogging(options =>
            {
                options.RequestBodyLogLimit = 1024 * 1024;
                options.ResponseBodyLogLimit = 1024 * 1024;
                options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
                options.MediaTypeOptions.AddText("application/json");
            });

            // 添加内存缓存服务 - SignalRNotificationService依赖此服务
            builder.Services.AddMemoryCache(options =>
            {
                // 针对SignalR Hub缓存的优化配置
                // 移除SizeLimit以避免每个缓存项都需要指定Size的复杂性
                // options.SizeLimit = 512; // 注释掉，Hub数量有限，不需要严格的大小限制
                options.CompactionPercentage = 0.10; // 仅清理10%，保持Hub上下文稳定
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(10); // 降低扫描频率，减少开销

                // .NET 8 新特性：设置内存压力阈值
                options.TrackLinkedCacheEntries = false; // 关闭链接追踪以提高性能
                options.TrackStatistics = builder.Environment.IsDevelopment(); // 仅开发环境追踪统计
            });

            //builder.Services.AddEFCoreArchiveDb(builder.Configuration);
            //builder.Services.AddEFCoreAndMySql(builder.Configuration);
            builder.Services.AddEFCoreAndPostgreSQL(builder.Configuration);

            // 添加 Lazy<T> 支持以解决循环依赖问题
            builder.Services.AddLazySupport();

            //!!!通过反射自动注册所有服务 不需要再单独写注册服务的代码 只需要在服务类上继承ITransientDependency、IScopedDependency、ISingletonDependency
            builder.Services.AddServices<ITransientDependency>(ServiceLifetime.Transient);
            builder.Services.AddServices<IScopedDependency>(ServiceLifetime.Scoped);
            builder.Services.AddServices<ISingletonDependency>(ServiceLifetime.Singleton);

            builder.Services.AddAliyunOSS(builder.Configuration);

            var cors = builder.Configuration["Cors"] ?? string.Empty;
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowSpecificOrigin",
                    builder => builder.WithOrigins(cors.Split(';').Select(x => x.Trim()).ToArray())
                                      .AllowAnyMethod()
                                      .AllowAnyHeader()
                                      .AllowCredentials());
            });

            if (coreServiceType == CoreServiceType.WebService)
            {
                //SSO认证
                //builder.Services.AddSsoAuthentication(builder.Configuration);
                //builder.Services.AddAuthentication("OpenAPIAuthentication")
                //    //开放平台开放API认证
                //    .AddScheme<RequestAuthenticationSchemeOptions, RequestAuthenticationHandler>(
                //    "OpenAPIAuthentication",
                //    options => { });

                builder.Services.AddAuthorization();

                // 添加SignalR服务 - 使用.NET 8最新功能
                builder.Services.AddSignalR(options =>
                {
                    // 配置SignalR选项
                    options.EnableDetailedErrors = builder.Environment.IsDevelopment();

                    // .NET 8 优化的超时设置
                    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);  // 增加到2分钟
                    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                    options.HandshakeTimeout = TimeSpan.FromSeconds(15);

                    // .NET 8 新增的性能优化选项
                    options.StreamBufferCapacity = 10;
                    options.MaximumReceiveMessageSize = 64 * 1024; // 增加到64KB以支持MessagePack
                    options.MaximumParallelInvocationsPerClient = 1;
                })
                // 添加JSON协议支持 - 默认协议，兼容性最好
                .AddJsonProtocol(options =>
                {
                    // 配置JSON序列化选项
                    options.PayloadSerializerOptions.Converters.Add(new JsonLongConverter());
                    options.PayloadSerializerOptions.Converters.Add(new JsonDecimalConverter());

                    // .NET 8 性能优化
                    options.PayloadSerializerOptions.DefaultBufferSize = 1024;
                    options.PayloadSerializerOptions.MaxDepth = 64;
                })
                // 添加MessagePack协议支持 - 高性能二进制协议
                .AddMessagePackProtocol(options =>
                {
                    // 配置MessagePack选项以获得最佳性能
                    options.SerializerOptions = MessagePack.MessagePackSerializerOptions.Standard
                        .WithCompression(MessagePack.MessagePackCompression.Lz4BlockArray)
                        .WithOldSpec(false)
                        .WithOmitAssemblyVersion(true);
                });
            }
            else if (coreServiceType == CoreServiceType.Gateway)
            {
                //SSO认证
                //builder.Services.AddSsoAuthentication(builder.Configuration);
                //builder.Services.AddAuthentication("OpenAPIAuthentication")
                //    //POS开放平台开放API认证
                //    .AddScheme<RequestAuthenticationSchemeOptions, RequestAuthenticationHandler>(
                //    "OpenAPIAuthentication",
                //    options => { });

                builder.Services.AddAuthorization();

                // 注册代理配置并发锁服务（单例，跨请求共享）
                builder.Services.AddSingleton<ProxyConfigLockService>();

                // 注册 JSON 配置存储（内存缓存 + 文件监听）
                builder.Services.AddSingleton<IProxyConfigStore, JsonProxyConfigStore>();
                builder.Services.AddSingleton<DynamicProxyConfigProvider>();

                // 使用自定义配置提供程序注册 YARP
                builder.Services.AddReverseProxy()
                    .LoadFromMemory(Array.Empty<RouteConfig>(),
                                    Array.Empty<ClusterConfig>())
                    .Services.AddSingleton<IProxyConfigProvider>(
                        sp => sp.GetRequiredService<DynamicProxyConfigProvider>());
            }

            builder.Services.AddControllers(options =>
            {
                //统一接口返回的处理
                options.Filters.Add<RequestAsyncResultFilter>();

                //接口异常统一处理
                options.Filters.Add<ApiAsyncExceptionFilter>();

                // 接口权限验证
                options.Filters.Add<RequestAuthorizeFilter>();

                // 操作日志统一处理
                options.Filters.Add<OperationLogActionFilter>();
            })
            .AddJsonOptions(options =>
            {
                // 针对字段 long 类型，序列化时转换为字符串
                options.JsonSerializerOptions.Converters.Add(new JsonLongConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonDecimalConverter());
                options.JsonSerializerOptions.Converters.Add(new EmptyStringToNullDateTimeConverter());
            });

            // 注册 HttpClient 服务（包含 IHttpClientFactory）
            builder.Services.AddHttpRequestClient();

            builder.Services.AddAllAutoMapper();

            builder.Services.AddRabbitMQ(builder.Configuration);

            // 指定文件的静态资源
            builder.Services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

            if (coreServiceType == CoreServiceType.PlanTaskService)
            {
                builder.Services.AddCronTask();
                //Quartz Job服务
                //builder.Services.AddQuartz(builder.Configuration);
                //POS BackService
                builder.Services.AddHostedService<ExecuteSeedDataService>();
            }
            //else
            //{
            //    builder.Services.AddHostedService<InitApiResourceService>();
            //}

            return builder;
        }

        /// <summary>
        /// 应用程序启动时 注册中间件
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseApp(this WebApplication app, string moduleKey, string moduleTitle, CoreServiceType coreServiceType = CoreServiceType.WebService)
        {
            App.Init(app.Services);

            //app.UseSetStartDefaultRoute();

            if (app.Environment.IsDevelopment() || app.Services.GetService<IOptions<SwaggerOptions>>()!.Value.Enable)
            {
                app.UseSwagger(moduleKey, moduleTitle);
            }

            app.UseDynamicLocalizer();

            app.UseMiddleware<ExceptionHandlerMiddleware>();

            app.UseHttpLogging();

            app.UseRedis(app.Configuration);

            // 添加默认静态文件支持（wwwroot）
            app.UseStaticFiles();

            // 添加上传文件的静态文件服务
            app.UseStaticFileServer();

            app.UseHttpsRedirection();

            app.UseCors("AllowSpecificOrigin");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            if (coreServiceType == CoreServiceType.MQService)
            {
                app.AddRabbitMQEventBus();
            }
            else if (coreServiceType == CoreServiceType.Gateway)
            {
                // 启用反向代理
                app.MapReverseProxy();
            }

            app.AddRabbitMQCodeManager();

            return app;
        }

        /// <summary>
        /// 初始化 Host，加载配置文件
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="moduleKey"></param>
        /// <returns></returns>
        public static IHostBuilder InitHostAndConfig(this IHostBuilder builder, string moduleKey, CoreServiceType coreServiceType = CoreServiceType.WebService)
        {
            Thread.CurrentThread.Name = moduleKey;

            // 最开始代码中没有使用到，是不会加载到内存中的，所以需要手动加载
            var assemblyFiles = Directory.GetFiles(AppContext.BaseDirectory, "NexusStack.*.dll");
            foreach (var assemblyFile in assemblyFiles)
            {
                AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
            }

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // 重新加载配置文件到 Host 的配置构建器
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false);
            });

            // 使用延迟配置的方式初始化 AgileConfig
            builder.UseAgileConfig(configBuilder =>
            {
                try
                {
                    // 构建临时配置以读取 AgileConfig 设置
                    var tempConfig = configBuilder.Build();
                    var agileConfigSection = tempConfig.GetSection("AgileConfig");

                    if (agileConfigSection.Exists())
                    {
                        // 手动构建 ConfigClientOptions
                        var options = new ConfigClientOptions
                        {
                            AppId = agileConfigSection["appId"],
                            Secret = agileConfigSection["secret"],
                            Nodes = agileConfigSection["nodes"],
                            Name = agileConfigSection["name"],
                            ENV = agileConfigSection["env"],
                            Tag = agileConfigSection["tag"],
                            CacheDirectory = agileConfigSection["cache:directory"]
                        };

                        // 处理 ServiceRegister
                        var serviceRegisterSection = agileConfigSection.GetSection("serviceRegister");
                        if (serviceRegisterSection.Exists())
                        {
                            options.RegisterInfo = new ServiceRegisterInfo
                            {
                                ServiceId = serviceRegisterSection["serviceId"],
                                ServiceName = serviceRegisterSection["serviceName"]
                            };
                        }

                        Console.WriteLine($"AgileConfig initialized for {moduleKey}:");
                        Console.WriteLine($"  AppId: {options.AppId}");
                        Console.WriteLine($"  Env: {options.ENV}");
                        Console.WriteLine($"  ServiceId: {options.RegisterInfo?.ServiceId}");
                        Console.WriteLine($"  ServiceName: {options.RegisterInfo?.ServiceName}");

                        return options;
                    }
                    else
                    {
                        Console.WriteLine($"AgileConfig section not found for {moduleKey}, using default options");
                        return new ConfigClientOptions(); // 返回默认配置
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing AgileConfig for {moduleKey}: {ex.Message}");
                    return new ConfigClientOptions(); // 返回默认配置
                }
            });

            builder.UseLog(coreServiceType);

            return builder;
        }

        /// <summary>
        /// 上传文件的静态文件服务5
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseStaticFileServer(this IApplicationBuilder app)
        {
            var storageOptions = app.ApplicationServices.GetService<IOptions<StorageOptions>>();
            var staticDirectory = Path.Combine(AppContext.BaseDirectory, storageOptions.Value.Path.IsNullOrEmpty() ? "uploads" : storageOptions.Value.Path);

            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            };
            forwardedHeadersOptions.KnownIPNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            forwardedHeadersOptions.ForwardLimit = null;
            app.UseForwardedHeaders(forwardedHeadersOptions);

            Directory.CreateDirectory(staticDirectory);

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                RequestPath = "/static",
                FileProvider = new PhysicalFileProvider(staticDirectory)
            });

            return app;
        }

        /// <summary>
        /// 添加 Lazy<T> 支持以解决循环依赖问题
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddLazySupport(this IServiceCollection services)
        {
            services.AddTransient(typeof(Lazy<>), typeof(LazyService<>));
            return services;
        }

        /// <summary>
        /// 注册所有 AutoMapper 配置信息
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAllAutoMapper(this IServiceCollection services)
        {
            var types = TypeFinders.SearchTypes(typeof(Profile), TypeFinders.TypeClassification.Class).ToArray();

            if (types.Length == 0)
            {
                Console.WriteLine("警告: 未找到任何 AutoMapper Profile 类型");
                return services;
            }

            // AutoMapper 16+ 推荐使用程序集方式注册
            var assemblies = types.Select(t => t.Assembly).Distinct().ToArray();

            Console.WriteLine($"注册 AutoMapper，共找到 {types.Length} 个 Profile，分布在 {assemblies.Length} 个程序集中");

            // 使用配置 Action 方式，逐个添加 Profile
            services.AddAutoMapper(cfg =>
            {
                foreach (var type in types)
                {
                    cfg.AddProfile(type);
                }
            });

            return services;
        }

        /// <summary>
        /// 自动注册所有实现 IOptions 的配置选项
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private static IServiceCollection ConfigureOptions(this IServiceCollection services, ConfigurationManager configuration)
        {
            PrintConfigurationProvider(configuration);

            services.AddOptions();

            // 注册所有 Options
            var types = TypeFinders.SearchTypes(typeof(IOptions), TypeFinders.TypeClassification.Interface);

            //using reflection to invoke the Configure<TOption> extension
            Type extensionClass = typeof(OptionsConfigurationServiceCollectionExtensions);
            //get the desired extension method by name and using the expected arguments
            Type[] parameterTypes = [typeof(IServiceCollection), typeof(IConfiguration)];
            string extensionName = nameof(OptionsConfigurationServiceCollectionExtensions.Configure);
            MethodInfo configureExtension = extensionClass.GetMethod(extensionName, parameterTypes);

            foreach (var optionType in types)
            {
                var instance = (IOptions)Activator.CreateInstance(optionType);

                IConfiguration section = instance.SectionName.IsNullOrEmpty() ? configuration : configuration.GetSection(instance.SectionName);
                MethodInfo extensionMethod = configureExtension.MakeGenericMethod(optionType);
                extensionMethod.Invoke(services, new object[] { services, section });
            }

            return services;
        }

        /// <summary>
        /// 程序启动时打印配置文件地址
        /// </summary>
        /// <param name="configuration"></param>
        private static void PrintConfigurationProvider(IConfiguration configuration)
        {
            var root = (IConfigurationBuilder)configuration;

            foreach (JsonConfigurationSource source in root.Sources.Where(a => a.GetType() == typeof(JsonConfigurationSource)).Cast<JsonConfigurationSource>())
            {
                var path = Path.Combine(((PhysicalFileProvider)source.FileProvider).Root, source.Path);
                //Log.Information($"配置文件({(File.Exists(path) ? "有效" : "无效")}):{path}");
                Console.WriteLine($"Configuration file({(File.Exists(path) ? "Effective" : "Invalid")}):{path}");
            }
        }

        /// <summary>
        /// 初始化定时任务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCronTask(this IServiceCollection services)
        {
            var cronType = typeof(CronScheduleService);

            foreach (var type in TypeFinders.SearchTypes(cronType, TypeFinders.TypeClassification.Class))
            {
                services.Add(new ServiceDescriptor(typeof(IHostedService), type, ServiceLifetime.Singleton));
            }
            return services;
        }

        /// <summary>
        /// Lazy<T> 服务实现类，用于支持依赖注入中的 Lazy<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class LazyService<T> : Lazy<T> where T : class
        {
            public LazyService(IServiceProvider serviceProvider)
                : base(serviceProvider.GetRequiredService<T>)
            {
            }
        }
    }
}
