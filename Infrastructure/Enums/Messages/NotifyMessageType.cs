using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Enums.Messages
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public enum NotifyMessageType
    {
        /// <summary>
        /// 系统内通知
        /// </summary>
        Insystem = 1,
        /// <summary>
        /// 邮件通知
        /// </summary>
        Email = 2,
        /// <summary>
        /// 短信通知
        /// </summary>
        SMS = 3,
    }
}
