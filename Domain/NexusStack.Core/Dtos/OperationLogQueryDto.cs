using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos
{
    public class OperationLogQueryDto : PagedQueryModelBase
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// 日志类型
        /// </summary>
        public LogType? LogType { get; set; }

        /// <summary>
        /// 操作模块
        /// </summary>
        public string? MenuCode { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long? UserId { get; set; }
    }
}
