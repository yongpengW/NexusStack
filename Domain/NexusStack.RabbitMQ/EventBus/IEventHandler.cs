using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.RabbitMQ.EventBus
{
    public interface IEventHandler
    {

    }

    public interface IEventHandler<TEvent> : IEventHandler
    {
        Task HandleAsync(TEvent @event);
    }
}
