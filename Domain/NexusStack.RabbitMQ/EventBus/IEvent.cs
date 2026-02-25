using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.RabbitMQ.EventBus
{
    /// <summary>
    /// 事件传输实体的基础接口
    /// </summary>
    public interface IEvent
    {
        object Id { get; set; }
        string TaskCode { get; }
    }
}
