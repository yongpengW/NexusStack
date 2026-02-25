using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace NexusStack.Core.Services.SignalR
{
    /// <summary>
    /// SignalR通知服务实现 - 优化版本
    /// </summary>
    public class SignalRNotificationService : ISignalRNotificationService, IScopedDependency
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SignalRNotificationService> _logger;
        private readonly IMemoryCache _cache;
        private readonly SignalRNotificationOptions _options;

        // 静态缓存Hub上下文类型映射，避免重复反射查找
        private static readonly ConcurrentDictionary<string, Type?> _hubTypeCache = new();

        public SignalRNotificationService(
            IServiceProvider serviceProvider,
            ILogger<SignalRNotificationService> logger,
            IMemoryCache cache,
            IOptions<SignalRNotificationOptions> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cache = cache;
            _options = options.Value;
        }

        /// <summary>
        /// 发送消息到指定Hub的核心方法，支持重试和性能优化
        /// </summary>
        private async Task<bool> SendToHubAsync(string hubPath, string methodName, string? groupName, object data)
        {
            var stopwatch = _options.EnablePerformanceMonitoring ? Stopwatch.StartNew() : null;

            for (int attempt = 0; attempt < _options.MaxRetries; attempt++)
            {
                try
                {
                    var hubContext = await GetHubContextAsync(hubPath);
                    if (hubContext == null)
                    {
                        if (_options.EnableDetailedLogging)
                            _logger.LogWarning("Hub context not found for path: {HubPath} (attempt {Attempt})", hubPath, attempt + 1);

                        if (attempt == _options.MaxRetries - 1) return false;
                        continue;
                    }

                    // 记录发送的消息用于调试
                    if (_options.EnableDetailedLogging)
                    {
                        _logger.LogDebug("发送SignalR消息: Hub={HubPath}, 方法={MethodName}, 组={GroupName}, 数据={Data}",
                            hubPath, methodName, groupName ?? "ALL", System.Text.Json.JsonSerializer.Serialize(data));
                    }

                    // 根据是否有组名决定发送目标
                    if (!string.IsNullOrEmpty(groupName))
                    {
                        await hubContext.Clients.Group(groupName).SendAsync(methodName, data);
                        if (_options.EnableDetailedLogging)
                            _logger.LogDebug("消息已发送到组 {GroupName}, 方法: {MethodName}, Hub路径: {HubPath}", groupName, methodName, hubPath);
                    }
                    else
                    {
                        await hubContext.Clients.All.SendAsync(methodName, data);
                        if (_options.EnableDetailedLogging)
                            _logger.LogDebug("消息已广播到所有客户端, 方法: {MethodName}, Hub路径: {HubPath}", methodName, hubPath);
                    }

                    // 记录性能指标
                    if (_options.EnablePerformanceMonitoring && stopwatch != null)
                    {
                        stopwatch.Stop();
                        if (stopwatch.ElapsedMilliseconds > 1000) // 超过1秒记录警告
                        {
                            _logger.LogWarning("SignalR消息发送耗时较长: {ElapsedMs}ms, Hub: {HubPath}, 方法: {MethodName}",
                                stopwatch.ElapsedMilliseconds, hubPath, methodName);
                        }
                        else if (_options.EnableDetailedLogging)
                        {
                            _logger.LogDebug("SignalR消息发送耗时: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                        }
                    }

                    return true;
                }
                catch (Exception ex) when (attempt < _options.MaxRetries - 1)
                {
                    if (_options.EnableDetailedLogging)
                        _logger.LogWarning(ex, "发送SignalR消息失败 (尝试 {Attempt}/{MaxRetries}): Hub路径={HubPath}, 方法={MethodName}",
                            attempt + 1, _options.MaxRetries, hubPath, methodName);

                    // 指数退避策略
                    await Task.Delay(TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * _options.BaseRetryDelayMs));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "发送SignalR消息最终失败: Hub路径={HubPath}, 方法={MethodName}, 数据={Data}",
                        hubPath, methodName, System.Text.Json.JsonSerializer.Serialize(data));
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取Hub上下文，带缓存优化
        /// </summary>
        private async Task<IHubContext?> GetHubContextAsync(string hubPath)
        {
            // 如果禁用缓存，直接获取
            if (!_options.EnableHubContextCaching)
            {
                return await GetHubContextDirectlyAsync(hubPath);
            }

            // 尝试从缓存获取Hub上下文
            var cacheKey = $"hub_context_{hubPath}";
            if (_cache.TryGetValue(cacheKey, out IHubContext? cachedContext) && cachedContext != null)
            {
                return cachedContext;
            }

            var hubContext = await GetHubContextDirectlyAsync(hubPath);
            if (hubContext != null)
            {
                // 缓存Hub Context
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.HubContextCacheExpiryMinutes),
                    SlidingExpiration = TimeSpan.FromMinutes(_options.HubContextCacheExpiryMinutes / 2),
                    Priority = CacheItemPriority.Normal
                    // 由于移除了SizeLimit，不再需要指定Size属性
                };

                _cache.Set(cacheKey, hubContext, cacheOptions);
            }

            return hubContext;
        }

        /// <summary>
        /// 直接获取Hub上下文，不使用缓存
        /// </summary>
        private async Task<IHubContext?> GetHubContextDirectlyAsync(string hubPath)
        {
            // 获取Hub名称映射
            if (!_options.HubPathMappings.TryGetValue(hubPath, out var hubNames))
            {
                if (_options.EnableDetailedLogging)
                    _logger.LogWarning("No hub mapping found for path: {HubPath}", hubPath);
                return null;
            }

            // 按优先级尝试获取Hub Context
            foreach (var hubName in hubNames)
            {
                var hubContext = await GetHubContextByNameAsync(hubName);
                if (hubContext != null)
                {
                    return hubContext;
                }
            }

            if (_options.EnableDetailedLogging)
                _logger.LogWarning("No available hub context found for path: {HubPath}", hubPath);
            return null;
        }

        /// <summary>
        /// 根据Hub名称获取Hub上下文，优化反射性能
        /// </summary>
        private async Task<IHubContext?> GetHubContextByNameAsync(string hubTypeName)
        {
            try
            {
                // 从静态缓存获取Hub类型
                var hubType = _hubTypeCache.GetOrAdd(hubTypeName, name => FindHubType(name));

                if (hubType == null)
                {
                    if (_options.EnableDetailedLogging)
                        _logger.LogWarning("Hub type not found: {HubTypeName}", hubTypeName);
                    return null;
                }

                // 构造IHubContext<T>类型并从DI容器获取
                var hubContextType = typeof(IHubContext<>).MakeGenericType(hubType);
                return _serviceProvider.GetService(hubContextType) as IHubContext;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hub context for: {HubTypeName}", hubTypeName);
                return null;
            }
        }

        /// <summary>
        /// 查找Hub类型的优化实现
        /// </summary>
        private static Type? FindHubType(string hubTypeName)
        {
            try
            {
                // 限制搜索范围到POS相关程序集，提高性能
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetName().Name?.StartsWith("POS.") == true);

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var hubType = assembly.GetTypes()
                            .FirstOrDefault(t => t.Name == hubTypeName &&
                                               typeof(Hub).IsAssignableFrom(t) &&
                                               !t.IsAbstract);

                        if (hubType != null)
                            return hubType;
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // 忽略类型加载异常，继续查找其他程序集
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                // 静默处理异常，返回null
            }

            return null;
        }

        /// <summary>
        /// 发送用户通知
        /// </summary>
        public async Task SendUserNotificationAsync(string userId, string title, string message, string notificationType = "info")
        {
            try
            {
                var notification = new
                {
                    Title = title,
                    Message = message,
                    Content = message, // 兼容前端
                    Type = notificationType,
                    UserId = userId,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var success = await SendToHubAsync("/hubs/notification", "UserNotification", $"user_{userId}", notification);

                if (success)
                {
                    _logger.LogInformation("发送用户通知成功: UserId={UserId}, Title={Title}", userId, title);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送用户通知异常: UserId={UserId}, Title={Title}", userId, title);
            }
        }

        /// <summary>
        /// 发送系统广播通知
        /// </summary>
        public async Task SendBroadcastNotificationAsync(string title, string message, string notificationType = "info")
        {
            try
            {
                var notification = new
                {
                    Title = title,
                    Message = message,
                    Content = message, // 兼容前端
                    Type = notificationType,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                var success = await SendToHubAsync("/hubs/notification", "BroadcastNotification", null, notification);

                if (success)
                {
                    _logger.LogInformation("发送系统广播通知成功: Title={Title}", title);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送系统广播通知异常: Title={Title}", title);
            }
        }
    }
}
