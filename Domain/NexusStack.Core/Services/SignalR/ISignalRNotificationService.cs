using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.SignalR
{
    /// <summary>
    /// SignalR通知服务接口
    /// </summary>
    public interface ISignalRNotificationService
    {
        /// <summary>
        /// 发送用户通知
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="title">通知标题</param>
        /// <param name="message">通知内容</param>
        /// <param name="notificationType">通知类型</param>
        /// <returns></returns>
        Task SendUserNotificationAsync(string userId, string title, string message, string notificationType = "info");

        /// <summary>
        /// 发送系统广播通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="message">通知内容</param>
        /// <param name="notificationType">通知类型</param>
        /// <returns></returns>
        Task SendBroadcastNotificationAsync(string title, string message, string notificationType = "info");
    }
}
