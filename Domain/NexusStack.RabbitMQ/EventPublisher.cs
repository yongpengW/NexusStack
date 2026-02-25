using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusStack.RabbitMQ.EventBus;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace NexusStack.RabbitMQ
{
    /// <summary>
    /// RabbitMQ事件发布者
    /// </summary>
    public class EventPublisher : IEventPublisher
    {
        private readonly IConnection connection;
        private readonly ILogger<EventPublisher> logger;
        private readonly RabbitOptions options;
        private IChannel publisherChannel;

        public EventPublisher(IConnection connection, ILogger<EventPublisher> logger, IOptions<RabbitOptions> options)
        {
            this.connection = connection;
            this.logger = logger;
            this.options = options.Value;
            this.publisherChannel = CreateChannelAsync().GetAwaiter().GetResult();
        }

        public async void Publish<TEvent>(TEvent message) where TEvent : IEvent
        {
            var eventName = message.GetType().FullName;
            var body = JsonSerializer.Serialize(message);

            await this.publisherChannel.BasicPublishAsync(
                exchange: this.options.ExchangeName,
                routingKey: eventName,
                body: Encoding.UTF8.GetBytes(body));
        }

        private async Task<IChannel> CreateChannelAsync()
        {
            var channel = await connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(
                exchange: this.options.ExchangeName,
                type: ExchangeType.Direct,
                durable: true);

            return channel;
        }
    }
}
