using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.RabbitMQ
{
    public interface IEventCodeManager
    {
        void RegisterEventType(string code, Type eventType);
        Type GetEventType(string code);
    }
}
