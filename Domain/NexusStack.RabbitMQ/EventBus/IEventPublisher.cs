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

        /// <summary>
        /// 发布延迟事件消息（TTL + DLX，使用固定档位）
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="message"></param>
        /// <param name="delayTier">延迟档位（1/3/5/10/30/60/120 分钟）</param>
        Task PublishDelayedAsync<TEvent>(TEvent message, DelayTier delayTier) where TEvent : IEvent;
    }
}
