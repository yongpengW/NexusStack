using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos
{
    public class CreateOperationLogDto
    {
        /// <summary>
        /// IP 地址
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 日志类型
        /// </summary>
        public LogType LogType { get; set; }

        /// <summary>
        /// User Agent
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// 操作
        /// </summary>
        public string OperationContent { get; set; }

        /// <summary>
        /// 菜单代码
        /// </summary>
        public string MenuCode { get; set; }

        /// <summary>
        /// 错误Tracker
        /// </summary>
        public string? ErrorTracker { get; set; }

        /// <summary>
        /// 操作菜单
        /// </summary>
        public string OperationMenu { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }

    public class SystemLogDto
    {
        public SystemLogDto()
        {
            logType = LogType.Info;
        }

        public string title { get; set; }
        public SystemLogContent entity { get; set; }
        public LogType logType { get; set; }
        public long userId { get; set; } = 0;
    }

    public class SystemLogContent
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Entity { get; set; }
    }
}
