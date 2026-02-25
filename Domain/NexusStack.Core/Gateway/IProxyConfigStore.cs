using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Gateway
{
    public interface IProxyConfigStore
    {
        Task<ProxyConfiguration> GetConfigAsync();
        Task SaveConfigAsync(ProxyConfiguration config);
        Task<ProxyRouteConfig?> GetRouteAsync(string routeId);
        Task<ProxyClusterConfig?> GetClusterAsync(string clusterId);
        Task AddOrUpdateRouteAsync(ProxyRouteConfig route);
        Task DeleteRouteAsync(string routeId);
        Task AddOrUpdateClusterAsync(ProxyClusterConfig cluster);
        Task DeleteClusterAsync(string clusterId);
    }
}
