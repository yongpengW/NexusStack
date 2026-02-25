using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.SystemManagement
{
    /// <summary>
    /// 操作日志
    /// </summary>
    public class OperationLog : AuditedEntity
    {
        /// <summary>
        /// IP 地址
        /// </summary>
        [MaxLength(128)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// 日志类型
        /// </summary>
        public LogType LogType { get; set; }

        /// <summary>
        /// User Agent
        /// </summary>
        [MaxLength(512)]
        public string? UserAgent { get; set; }

        /// <summary>
        /// 操作
        /// </summary>
        public string? OperationContent { get; set; }

        /// <summary>
        /// 菜单代码
        /// </summary>
        [MaxLength(128)]
        public string? MenuCode { get; set; }

        /// <summary>
        /// 错误Tracker
        /// </summary>
        public string? ErrorTracker { get; set; }

        /// <summary>
        /// 操作菜单
        /// </summary>
        public string? OperationMenu { get; set; }

        /// <summary>
        /// 请求方法
        /// </summary>
        public string? Method { get; set; }
    }
}
