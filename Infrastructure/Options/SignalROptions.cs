using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Options
{
    /// <summary>
    /// SignalR通知服务配置选项
    /// </summary>
    public class SignalROptions : IOptions
    {
        /// <summary>
        /// 配置节名称
        /// </summary>
        public string SectionName => "SignalR";

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// 重试基础延迟（毫秒）
        /// </summary>
        public int BaseRetryDelayMs { get; set; } = 100;
    }
}
