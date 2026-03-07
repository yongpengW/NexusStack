using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusStack.RabbitMQ.EventBus;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NexusStack.RabbitMQ
{
    /// <summary>
    /// RabbitMQ事件发布者
    /// </summary>
    public class EventPublisher : IEventPublisher, IDisposable
    {
        private readonly IConnection connection;
        private readonly ILogger<EventPublisher> logger;
        private readonly RabbitOptions options;
        private readonly SemaphoreSlim publishLock = new(1, 1);
        private IChannel publisherChannel;

        public EventPublisher(IConnection connection, ILogger<EventPublisher> logger, IOptions<RabbitOptions> options)
        {
            this.connection = connection;
            this.logger = logger;
            this.options = options.Value;
            this.publisherChannel = CreateChannelAsync().GetAwaiter().GetResult();

            this.publisherChannel.BasicReturnAsync += async (_, args) =>
            {
                var returnedBody = Encoding.UTF8.GetString(args.Body.ToArray());
                this.logger.LogError($"消息路由失败并被退回。Exchange:{args.Exchange}, RoutingKey:{args.RoutingKey}, ReplyCode:{args.ReplyCode}, ReplyText:{args.ReplyText}, Body:{returnedBody}");
                await Task.CompletedTask;
            };
        }

        public Task PublishAsync<TEvent>(TEvent message) where TEvent : IEvent
        {
            return PublishInternalAsync(message);
        }

        private async Task PublishInternalAsync<TEvent>(TEvent message) where TEvent : IEvent
        {
            await this.publishLock.WaitAsync();
            try
            {
                var eventName = message.GetType().FullName;
                var body = JsonSerializer.Serialize(message);

                var properties = new BasicProperties
                {
                    Persistent = true
                };

                await this.publisherChannel.BasicPublishAsync(
                    exchange: this.options.ExchangeName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: Encoding.UTF8.GetBytes(body));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "发布 RabbitMQ 消息失败");
                throw;
            }
            finally
            {
                this.publishLock.Release();
            }
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

        public void Dispose()
        {
            try
            {
                if (this.publisherChannel is not null)
                {
                    this.publisherChannel.CloseAsync().GetAwaiter().GetResult();
                    this.publisherChannel.Dispose();
                }
            }
            finally
            {
                this.publishLock.Dispose();
            }
        }
    }
}
