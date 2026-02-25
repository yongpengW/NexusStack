using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.RabbitMQ.EventBus
{
    /// <summary>
    /// RabbitMQ订阅者接口
    /// </summary>
    public interface IEventSubscriber : IDisposable
    {
        void Subscribe(Type eventType, Type eventHandlerType);
    }
}
