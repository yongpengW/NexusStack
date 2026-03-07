using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NexusStack.RabbitMQ.EventBus
{
    /// <summary>
    /// RabbitMQ 发布接口
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// 发布事件消息
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="message"></param>
        Task PublishAsync<TEvent>(TEvent message) where TEvent : IEvent;
    }
}
