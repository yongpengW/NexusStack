using Microsoft.AspNetCore.Mvc;
using NexusStack.Core.Attributes;
using NexusStack.Core.Gateway;
using NexusStack.Infrastructure.Exceptions;

namespace NexusStack.Gateway.Controllers;

public class ProxyManagementController : BaseController
{
    private readonly IProxyConfigStore _configStore;
    private readonly DynamicProxyConfigProvider _configProvider;
    private readonly ILogger<ProxyManagementController> _logger;
    private readonly ProxyConfigLockService _lockService;

    public ProxyManagementController(
        IProxyConfigStore configStore,
        DynamicProxyConfigProvider configProvider,
        ILogger<ProxyManagementController> logger,
        ProxyConfigLockService lockService)
    {
        _configStore = configStore;
        _configProvider = configProvider;
        _logger = logger;
        _lockService = lockService;
    }

    #region Routes Management

    [HttpGet("routes"), NoLogging]
    public async Task<List<ProxyRouteConfig>> GetAllRoutes()
    {
        var config = await _configStore.GetConfigAsync();
        return config.Routes;
    }

    [HttpGet("routes/{routeId}"), NoLogging]
    public async Task<ProxyRouteConfig> GetRoute(string routeId)
    {
        var route = await _configStore.GetRouteAsync(routeId);
        if (route == null)
        {
            throw new BusinessException($"未找到路由: {routeId}");
        }
        return route;
    }

    [HttpPost("routes")]
    public async Task<StatusCodeResult> CreateRoute([FromBody] ProxyRouteConfig route)
    {
        if (string.IsNullOrEmpty(route.RouteId))
        {
            throw new BusinessException($"RouteId 不能为空");
        }

        await _lockService.Lock.WaitAsync();
        try
        {
            var existing = await _configStore.GetRouteAsync(route.RouteId);
            if (existing != null)
            {
                throw new BusinessException($"路由 {route.RouteId} 已存在");
            }

            await _configStore.AddOrUpdateRouteAsync(route);
            await _configProvider.ReloadConfigAsync();
            return Ok();
        }
        finally
        {
            _lockService.Lock.Release();
        }
    }

    [HttpPut("routes/{routeId}")]
    public async Task<StatusCodeResult> UpdateRoute(string routeId, [FromBody] ProxyRouteConfig route)
    {
        if (routeId != route.RouteId)
        {
            throw new BusinessException($"路由ID不匹配");
        }

        await _lockService.Lock.WaitAsync();
        try
        {
            var existing = await _configStore.GetRouteAsync(routeId);
            if (existing == null)
            {
                throw new BusinessException($"未找到路由: {routeId}");
            }

            await _configStore.AddOrUpdateRouteAsync(route);
            await _configProvider.ReloadConfigAsync();
            return Ok();
        }
        finally
        {
            _lockService.Lock.Release();
        }
    }

    [HttpDelete("routes/{routeId}")]
    public async Task<StatusCodeResult> DeleteRoute(string routeId)
    {
        await _lockService.Lock.WaitAsync();
        try
        {
            var existing = await _configStore.GetRouteAsync(routeId);
            if (existing == null)
            {
                throw new BusinessException($"未找到路由: {routeId}");
            }

            await _configStore.DeleteRouteAsync(routeId);
            await _configProvider.ReloadConfigAsync();

            return Ok();
        }
        finally
        {
            _lockService.Lock.Release();
        }
    }

    #endregion

    #region Clusters Management

    [HttpGet("clusters"), NoLogging]
    public async Task<List<ProxyClusterConfig>> GetAllClusters()
    {
        var config = await _configStore.GetConfigAsync();
        return config.Clusters;
    }

    [HttpGet("clusters/{clusterId}"), NoLogging]
    public async Task<ProxyClusterConfig> GetCluster(string clusterId)
    {
        var cluster = await _configStore.GetClusterAsync(clusterId);
        if (cluster == null)
        {
            throw new BusinessException($"未找到集群: {clusterId}");
        }
        return cluster;
    }

    [HttpPost("clusters")]
    public async Task<StatusCodeResult> CreateCluster([FromBody] ProxyClusterConfig cluster)
    {
        if (string.IsNullOrEmpty(cluster.ClusterId))
        {
            throw new BusinessException($"ClusterId 不能为空");
        }

        await _lockService.Lock.WaitAsync();
        try
        {
            var existing = await _configStore.GetClusterAsync(cluster.ClusterId);
            if (existing != null)
            {
                throw new BusinessException($"集群 {cluster.ClusterId} 已存在");
            }

            await _configStore.AddOrUpdateClusterAsync(cluster);
            await _configProvider.ReloadConfigAsync();

            return Ok();
        }
        finally
        {
            _lockService.Lock.Release();
        }
    }

    [HttpPut("clusters/{clusterId}")]
    public async Task<StatusCodeResult> UpdateCluster(string clusterId, [FromBody] ProxyClusterConfig cluster)
    {
        if (clusterId != cluster.ClusterId)
        {
            throw new BusinessException($"集群ID不匹配");
        }

        await _lockService.Lock.WaitAsync();
        try
        {
            var existing = await _configStore.GetClusterAsync(clusterId);
            if (existing == null)
            {
                throw new BusinessException($"未找到集群: {clusterId}");
            }

            await _configStore.AddOrUpdateClusterAsync(cluster);
            await _configProvider.ReloadConfigAsync();

            return Ok();
        }
        finally
        {
            _lockService.Lock.Release();
        }
    }

    [HttpDelete("clusters/{clusterId}")]
    public async Task<StatusCodeResult> DeleteCluster(string clusterId)
    {
        await _lockService.Lock.WaitAsync();
        try
        {
            var existing = await _configStore.GetClusterAsync(clusterId);
            if (existing == null)
            {
                throw new BusinessException($"未找到集群: {clusterId}");
            }

            await _configStore.DeleteClusterAsync(clusterId);
            await _configProvider.ReloadConfigAsync();

            return Ok();
        }
        finally
        {
            _lockService.Lock.Release();
        }
    }

    #endregion

    #region Full Configuration

    [HttpGet("config"), NoLogging]
    public async Task<ProxyConfiguration> GetFullConfig()
    {
        var config = await _configStore.GetConfigAsync();
        return config;
    }

    [HttpPost("reload")]
    public async Task<StatusCodeResult> ReloadConfig()
    {
        await _configProvider.ReloadConfigAsync();
        return Ok();
    }

    #endregion
}
