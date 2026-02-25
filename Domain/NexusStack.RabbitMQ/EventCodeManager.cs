using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.RabbitMQ
{
    public class EventCodeManager : IEventCodeManager
    {
        private readonly Dictionary<string, Type> _eventCodeMapping = [];

        public void RegisterEventType(string code, Type eventType)
        {
            if (!_eventCodeMapping.ContainsKey(code))
            {
                _eventCodeMapping[code] = eventType;
            }
        }

        public Type GetEventType(string code)
        {
            return _eventCodeMapping.TryGetValue(code, out var eventType) ? eventType : null;
        }
    }
}
