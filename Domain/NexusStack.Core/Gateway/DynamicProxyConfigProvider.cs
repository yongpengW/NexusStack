using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Yarp.ReverseProxy.Configuration;

namespace NexusStack.Core.Gateway
{
    public class DynamicProxyConfigProvider : IProxyConfigProvider, IDisposable
    {
        private readonly IProxyConfigStore _configStore;
        private readonly ILogger<DynamicProxyConfigProvider> _logger;
        private volatile DynamicProxyConfig _config;
        private readonly SemaphoreSlim _reloadLock = new(1, 1);
        private volatile bool _disposed = false;

        public DynamicProxyConfigProvider(
            IProxyConfigStore configStore,
            ILogger<DynamicProxyConfigProvider> logger)
        {
            _configStore = configStore;
            _logger = logger;

            // 同步加载初始配置（避免异步初始化问题）
            try
            {
                var initialConfig = LoadConfigSyncInternal();
                _config = new DynamicProxyConfig(initialConfig.routes, initialConfig.clusters);
                _logger.LogInformation("DynamicProxyConfigProvider 已初始化");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化加载配置失败，使用空配置");
                _config = new DynamicProxyConfig(new List<RouteConfig>(), new List<ClusterConfig>());
            }
        }

        public IProxyConfig GetConfig() => _config;

        public async Task ReloadConfigAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DynamicProxyConfigProvider));
            }

            // 等待锁，确保重载完成后才返回
            await _reloadLock.WaitAsync();
            try
            {
                if (_disposed) return;
                await LoadConfigAsync();
            }
            finally
            {
                _reloadLock.Release();
            }
        }

        /// <summary>
        /// 同步加载配置（用于构造函数初始化）
        /// </summary>
        private (List<RouteConfig> routes, List<ClusterConfig> clusters) LoadConfigSyncInternal()
        {
            // 使用 Task.Run 避免潜在的死锁问题
            var storedConfig = Task.Run(async () => await _configStore.GetConfigAsync()).GetAwaiter().GetResult();
            return ConvertConfig(storedConfig);
        }

        private async Task LoadConfigAsync()
        {
            try
            {
                var storedConfig = await _configStore.GetConfigAsync();
                var (routes, clusters) = ConvertConfig(storedConfig);

                var oldConfig = _config;
                _config = new DynamicProxyConfig(routes, clusters);

                // 先通知变更，再释放旧配置
                oldConfig.SignalChange();

                // 延迟释放旧配置，确保所有引用者都切换到新配置
                _ = Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    oldConfig.Dispose();
                });

                _logger.LogInformation("成功加载代理配置，Routes: {RouteCount}, Clusters: {ClusterCount}",
                    routes.Count, clusters.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载代理配置失败");
            }
        }

        /// <summary>
        /// 转换配置格式
        /// </summary>
        private (List<RouteConfig> routes, List<ClusterConfig> clusters) ConvertConfig(ProxyConfiguration storedConfig)
        {
            var routes = storedConfig.Routes.Select(r => new RouteConfig
            {
                RouteId = r.RouteId,
                ClusterId = r.ClusterId,
                Match = new RouteMatch
                {
                    Path = r.Path,
                    Headers = r.Headers?.Select(h => new RouteHeader
                    {
                        Name = h.Key,
                        Values = new[] { h.Value }
                    }).ToList(),
                    Methods = r.Methods
                },
                Order = r.Order,
                Metadata = r.Metadata
            }).ToList();

            var clusters = storedConfig.Clusters.Select(c => new ClusterConfig
            {
                ClusterId = c.ClusterId,
                Destinations = c.Destinations.ToDictionary(
                    d => d.Key,
                    d => new DestinationConfig
                    {
                        Address = d.Value.Address,
                        Health = d.Value.Health,
                        Metadata = d.Value.Metadata
                    }),
                LoadBalancingPolicy = c.LoadBalancingPolicy,
                HealthCheck = c.HealthCheck != null ? new Yarp.ReverseProxy.Configuration.HealthCheckConfig
                {
                    Active = new ActiveHealthCheckConfig
                    {
                        Enabled = c.HealthCheck.Enabled,
                        Interval = string.IsNullOrEmpty(c.HealthCheck.Interval)
                            ? null
                            : TimeSpan.Parse(c.HealthCheck.Interval),
                        Timeout = string.IsNullOrEmpty(c.HealthCheck.Timeout)
                            ? null
                            : TimeSpan.Parse(c.HealthCheck.Timeout),
                        Path = c.HealthCheck.Path
                    }
                } : null,
                Metadata = c.Metadata
            }).ToList();

            return (routes, clusters);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _reloadLock?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // 已经释放，忽略
            }

            try
            {
                _config?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // 已经释放，忽略
            }
        }
    }
}
