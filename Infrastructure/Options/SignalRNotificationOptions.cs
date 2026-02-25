using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Options
{
    /// <summary>
    /// SignalR通知服务配置选项
    /// </summary>
    public class SignalRNotificationOptions : IOptions
    {
        /// <summary>
        /// 配置节名称
        /// </summary>
        public string SectionName => "SignalRNotification";

        /// <summary>
        /// Hub上下文缓存过期时间（分钟）
        /// </summary>
        public int HubContextCacheExpiryMinutes { get; set; } = 5;

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// 重试基础延迟（毫秒）
        /// </summary>
        public int BaseRetryDelayMs { get; set; } = 100;

        /// <summary>
        /// 是否启用详细日志
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = false;

        /// <summary>
        /// 是否启用性能监控
        /// </summary>
        public bool EnablePerformanceMonitoring { get; set; } = true;

        /// <summary>
        /// Hub路径映射配置
        /// </summary>
        public Dictionary<string, string[]> HubPathMappings { get; set; } = new()
        {
            ["/hubs/notification"] = new[] { "NotificationHub" }
        };

        /// <summary>
        /// 并发发送超时时间（秒）
        /// </summary>
        public int ConcurrentSendTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 是否启用Hub上下文缓存
        /// </summary>
        public bool EnableHubContextCaching { get; set; } = true;

        /// <summary>
        /// Hub连接超时时间（秒）
        /// </summary>
        public int HubConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// 消息发送超时时间（秒）
        /// </summary>
        public int MessageSendTimeoutSeconds { get; set; } = 15;

        /// <summary>
        /// 是否启用消息批处理
        /// </summary>
        public bool EnableMessageBatching { get; set; } = false;

        /// <summary>
        /// 批处理消息最大数量
        /// </summary>
        public int BatchMessageMaxCount { get; set; } = 100;

        /// <summary>
        /// 批处理等待时间（毫秒）
        /// </summary>
        public int BatchWaitTimeMs { get; set; } = 100;
    }
}
