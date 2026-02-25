using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using Yarp.ReverseProxy.Configuration;

namespace NexusStack.Core.Gateway
{
    /// <summary>
    /// 动态代理配置包装器
    /// 实现 IProxyConfig 接口，支持配置变更通知和资源释放
    /// </summary>
    internal class DynamicProxyConfig : IProxyConfig, IDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private volatile bool _disposed = false;

        public DynamicProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            Routes = routes ?? throw new ArgumentNullException(nameof(routes));
            Clusters = clusters ?? throw new ArgumentNullException(nameof(clusters));
            ChangeToken = new CancellationChangeToken(_cts.Token);
        }

        public IReadOnlyList<RouteConfig> Routes { get; }
        public IReadOnlyList<ClusterConfig> Clusters { get; }
        public IChangeToken ChangeToken { get; }

        /// <summary>
        /// 触发配置变更通知
        /// </summary>
        public void SignalChange()
        {
            if (_disposed) return;

            try
            {
                if (!_cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {
                // CTS 已经被释放，忽略
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _cts?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // 已经释放，忽略
            }
        }
    }
}
