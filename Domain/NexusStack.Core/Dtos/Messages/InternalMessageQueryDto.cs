using NexusStack.Infrastructure.Enums.Messages;
using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Messages
{
    public class InternalMessageQueryDto : PagedQueryModelBase
    {
        public long? StoreId { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType? MessageType { get; set; }

        /// <summary>
        /// 系统类型
        /// </summary>
        public string? PlatformTypes { get; set; }

        /// <summary>
        /// 已读状态
        /// </summary>
        public bool? ReadStatus { get; set; }
    }
}
