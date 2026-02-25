using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.RabbitMQ.EventBus
{
    /// <summary>
    /// 事件传递数据基类
    /// </summary>
    public abstract class EventBase : IEvent
    {
        public object Id { get; set; } = Guid.NewGuid();
        public abstract string TaskCode { get; }
        public abstract long TaskId { get; set; }
        public abstract string Data { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
