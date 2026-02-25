using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.NotifyEvent
{
    public class CreateNotifyEventDto
    {
        /// <summary>
        /// 通知类型
        /// </summary>
        public List<long> MessageTypes { get; set; }
        /// <summary>
        /// 通知角色
        public List<long> NotifyRoles { get; set; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsActive { get; set; }
    }

    public class NotifyEventAlertDto
    {
        public string Title { get; set; }
        public string Message { get; set; }

        public List<long> StoreIds { get; set; } = new List<long>();
    }
}
