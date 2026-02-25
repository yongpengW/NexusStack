using NexusStack.Infrastructure.Enums.Messages;
using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Messages
{
    public class SMSQueryDto : PagedQueryModelBase
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 信息状态
        /// </summary>
        public MessageStatus? MessageStatus { get; set; }

        public MessageType? MessageType { get; set; }
    }
}
