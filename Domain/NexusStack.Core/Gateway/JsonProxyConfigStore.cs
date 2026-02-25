using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace NexusStack.Core.Gateway
{
    /// <summary>
    /// JSON 配置存储：内存缓存 + 文件监听
    /// 性能接近纯内存，保持 JSON 文件的简单性
    /// </summary>
    public class JsonProxyConfigStore : IProxyConfigStore, IDisposable
    {
        private readonly string _configPath;
        private readonly ILogger<JsonProxyConfigStore> _logger;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private readonly FileSystemWatcher _fileWatcher;

        // 内存缓存 - 使用 volatile 确保线程可见性
        private volatile ProxyConfiguration? _cachedConfig;
        private long _lastLoadTimeTicks = DateTime.MinValue.Ticks;

        // 防抖：记录上次写入时间，避免 SaveConfig 触发 FileSystemWatcher
        private long _lastSaveTimeTicks = DateTime.MinValue.Ticks;

        // 防抖：FileSystemWatcher 去重
        private long _lastFileChangeTimeTicks = DateTime.MinValue.Ticks;
        private readonly TimeSpan _fileChangeDebounce = TimeSpan.FromMilliseconds(300);

        // Dispose 标志
        private volatile bool _disposed = false;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        public JsonProxyConfigStore(ILogger<JsonProxyConfigStore> logger)
        {
            _logger = logger;
            _configPath = Path.Combine(AppContext.BaseDirectory, "proxy-config.json");

            // 确保配置文件存在
            EnsureConfigFileExists();

            // 同步加载初始配置（避免异步初始化问题）
            try
            {
                _cachedConfig = LoadConfigSync();
                _logger.LogInformation("JSON 配置存储已初始化: {Path}", _configPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化加载配置失败，使用空配置");
                _cachedConfig = new ProxyConfiguration();
            }

            // 监听文件变化
            _fileWatcher = CreateFileWatcher();
        }

        /// <summary>
        /// 获取配置（优先从内存缓存）
        /// </summary>
        public async Task<ProxyConfiguration> GetConfigAsync()
        {
            ThrowIfDisposed();

            // 快速路径：直接返回缓存（无锁，极快 <5μs）
            var cachedConfig = _cachedConfig;
            if (cachedConfig != null)
            {
                return cachedConfig;
            }

            // 慢速路径：加载配置
            await _lock.WaitAsync();
            try
            {
                // 双重检查
                cachedConfig = _cachedConfig;
                if (cachedConfig != null)
                {
                    return cachedConfig;
                }

                return await LoadConfigAsync();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// 同步加载配置（用于构造函数初始化）
        /// </summary>
        private ProxyConfiguration LoadConfigSync()
        {
            if (!File.Exists(_configPath))
            {
                _logger.LogWarning("配置文件不存在，返回空配置");
                return new ProxyConfiguration();
            }

            var json = File.ReadAllText(_configPath);
            return JsonSerializer.Deserialize<ProxyConfiguration>(json, _jsonOptions)
                   ?? new ProxyConfiguration();
        }

        /// <summary>
        /// 从文件加载配置并缓存（带重试机制）
        /// </summary>
        private async Task<ProxyConfiguration> LoadConfigAsync()
        {
            const int maxRetries = 3;
            const int retryDelayMs = 100;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    if (!File.Exists(_configPath))
                    {
                        _logger.LogWarning("配置文件不存在，返回空配置");
                        _cachedConfig = new ProxyConfiguration();
                        return _cachedConfig;
                    }

                    // 尝试读取文件
                    var json = await File.ReadAllTextAsync(_configPath);
                    var config = JsonSerializer.Deserialize<ProxyConfiguration>(json, _jsonOptions)
                                 ?? new ProxyConfiguration();

                    _cachedConfig = config;
                    Interlocked.Exchange(ref _lastLoadTimeTicks, DateTime.UtcNow.Ticks);

                    _logger.LogInformation(
                        "从文件加载配置成功，Routes: {RouteCount}, Clusters: {ClusterCount}",
                        config.Routes.Count,
                        config.Clusters.Count);

                    return config;
                }
                catch (IOException ioEx) when (attempt < maxRetries - 1)
                {
                    // 文件被锁定，等待后重试
                    _logger.LogWarning(ioEx, "读取配置文件失败（尝试 {Attempt}/{MaxRetries}），等待 {Delay}ms 后重试",
                        attempt + 1, maxRetries, retryDelayMs);
                    await Task.Delay(retryDelayMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "加载配置文件失败");
                    return _cachedConfig ?? new ProxyConfiguration();
                }
            }

            // 所有重试都失败
            _logger.LogError("加载配置文件失败（已重试 {MaxRetries} 次），使用缓存配置", maxRetries);
            return _cachedConfig ?? new ProxyConfiguration();
        }

        /// <summary>
        /// 保存配置到文件并刷新缓存
        /// </summary>
        public async Task SaveConfigAsync(ProxyConfiguration config)
        {
            ThrowIfDisposed();

            await _lock.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(config, _jsonOptions);
                await File.WriteAllTextAsync(_configPath, json);

                // 立即更新缓存
                _cachedConfig = config;
                var now = DateTime.UtcNow;
                Interlocked.Exchange(ref _lastLoadTimeTicks, now.Ticks);
                Interlocked.Exchange(ref _lastSaveTimeTicks, now.Ticks); // 记录保存时间，防止触发 FileSystemWatcher

                _logger.LogInformation("配置已保存并更新缓存");
            }
            finally
            {
                _lock.Release();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(JsonProxyConfigStore));
            }
        }

        public async Task<ProxyRouteConfig?> GetRouteAsync(string routeId)
        {
            var config = await GetConfigAsync();
            return config.Routes.FirstOrDefault(r => r.RouteId == routeId);
        }

        public async Task<ProxyClusterConfig?> GetClusterAsync(string clusterId)
        {
            var config = await GetConfigAsync();
            return config.Clusters.FirstOrDefault(c => c.ClusterId == clusterId);
        }

        public async Task AddOrUpdateRouteAsync(ProxyRouteConfig route)
        {
            var config = await GetConfigAsync();

            // 创建新的配置对象（避免修改缓存）
            var newConfig = new ProxyConfiguration
            {
                Routes = new List<ProxyRouteConfig>(config.Routes),
                Clusters = new List<ProxyClusterConfig>(config.Clusters)
            };

            var existing = newConfig.Routes.FirstOrDefault(r => r.RouteId == route.RouteId);
            if (existing != null)
            {
                newConfig.Routes.Remove(existing);
            }
            newConfig.Routes.Add(route);

            await SaveConfigAsync(newConfig);
        }

        public async Task DeleteRouteAsync(string routeId)
        {
            var config = await GetConfigAsync();

            var newConfig = new ProxyConfiguration
            {
                Routes = new List<ProxyRouteConfig>(config.Routes),
                Clusters = new List<ProxyClusterConfig>(config.Clusters)
            };

            var route = newConfig.Routes.FirstOrDefault(r => r.RouteId == routeId);
            if (route != null)
            {
                newConfig.Routes.Remove(route);
                await SaveConfigAsync(newConfig);
            }
        }

        public async Task AddOrUpdateClusterAsync(ProxyClusterConfig cluster)
        {
            var config = await GetConfigAsync();

            var newConfig = new ProxyConfiguration
            {
                Routes = new List<ProxyRouteConfig>(config.Routes),
                Clusters = new List<ProxyClusterConfig>(config.Clusters)
            };

            var existing = newConfig.Clusters.FirstOrDefault(c => c.ClusterId == cluster.ClusterId);
            if (existing != null)
            {
                newConfig.Clusters.Remove(existing);
            }
            newConfig.Clusters.Add(cluster);

            await SaveConfigAsync(newConfig);
        }

        public async Task DeleteClusterAsync(string clusterId)
        {
            var config = await GetConfigAsync();

            var newConfig = new ProxyConfiguration
            {
                Routes = new List<ProxyRouteConfig>(config.Routes),
                Clusters = new List<ProxyClusterConfig>(config.Clusters)
            };

            var cluster = newConfig.Clusters.FirstOrDefault(c => c.ClusterId == clusterId);
            if (cluster != null)
            {
                newConfig.Clusters.Remove(cluster);
                await SaveConfigAsync(newConfig);
            }
        }

        /// <summary>
        /// 创建文件监听器（支持外部编辑文件后自动重载）
        /// </summary>
        private FileSystemWatcher CreateFileWatcher()
        {
            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(_configPath)!,
                Filter = Path.GetFileName(_configPath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            // 文件变化时重新加载（注意：这是同步事件，但处理程序是异步的）
            watcher.Changed += OnFileChanged;

            return watcher;
        }

        /// <summary>
        /// 文件变化事件处理器（使用同步方法避免 async void）
        /// </summary>
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // 使用 Task.Run 避免阻塞 FileSystemWatcher 线程
            _ = Task.Run(async () =>
            {
                try
                {
                    if (_disposed) return;

                    var now = DateTime.UtcNow;
                    var lastSaveTime = new DateTime(Interlocked.Read(ref _lastSaveTimeTicks));
                    var lastFileChangeTime = new DateTime(Interlocked.Read(ref _lastFileChangeTimeTicks));

                    // 防抖1：如果是刚保存的（500ms 内），忽略（避免 SaveConfig 触发重载）
                    if ((now - lastSaveTime).TotalMilliseconds < 500)
                    {
                        _logger.LogDebug("忽略自身保存触发的文件变化");
                        return;
                    }

                    // 防抖2：防止重复触发（300ms 内只处理一次）
                    if ((now - lastFileChangeTime) < _fileChangeDebounce)
                    {
                        _logger.LogDebug("忽略重复的文件变化事件");
                        return;
                    }

                    Interlocked.Exchange(ref _lastFileChangeTimeTicks, now.Ticks);

                    // 延迟一小段时间，确保文件写入完成
                    await Task.Delay(100);

                    if (_disposed) return;

                    _logger.LogInformation("检测到外部配置文件变化，重新加载");

                    await _lock.WaitAsync();
                    try
                    {
                        if (_disposed) return;
                        await LoadConfigAsync();
                    }
                    finally
                    {
                        _lock.Release();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // 对象已释放，忽略
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理文件变化事件失败");
                }
            });
        }

        /// <summary>
        /// 确保配置文件存在
        /// </summary>
        private void EnsureConfigFileExists()
        {
            if (!File.Exists(_configPath))
            {
                var defaultConfig = new ProxyConfiguration();
                var json = JsonSerializer.Serialize(defaultConfig, _jsonOptions);
                File.WriteAllText(_configPath, json);

                _logger.LogInformation("创建默认配置文件: {Path}", _configPath);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            // 先停止文件监听
            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Changed -= OnFileChanged;
                _fileWatcher.Dispose();
            }

            // 尝试获取锁，但不等待太久（避免阻塞关闭流程）
            if (_lock != null)
            {
                if (_lock.Wait(TimeSpan.FromSeconds(1)))
                {
                    try
                    {
                        _lock.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        // 已经释放，忽略
                    }
                }
                else
                {
                    _logger.LogWarning("Dispose 时无法在 1 秒内获取锁，强制释放资源");
                    try
                    {
                        _lock.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                        // 已经释放，忽略
                    }
                }
            }
        }
    }
}
