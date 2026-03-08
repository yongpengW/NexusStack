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
    public class EventPublisher : IEventPublisher, IDisposable, IAsyncDisposable
    {
        private readonly IConnection connection;
        private readonly ILogger<EventPublisher> logger;
        private readonly RabbitOptions options;
        private readonly SemaphoreSlim publishLock = new(1, 1);
        private IChannel publisherChannel;
        private int disposed;

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
            if (Interlocked.CompareExchange(ref this.disposed, 0, 0) == 1)
            {
                throw new ObjectDisposedException(nameof(EventPublisher));
            }

            await this.publishLock.WaitAsync();
            try
            {
                // 锁内再次检查 disposed
                if (Interlocked.CompareExchange(ref this.disposed, 0, 0) == 1)
                {
                    throw new ObjectDisposedException(nameof(EventPublisher));
                }

                var eventName = message.GetType().FullName;
                var body = JsonSerializer.Serialize(message);

                var messageId = message is EventBase eventBase && eventBase.TaskId > 0
                    ? $"{message.TaskCode}:{eventBase.TaskId}"
                    : $"{message.TaskCode}:{message.Id}";

                var properties = new BasicProperties
                {
                    Persistent = true,
                    MessageId = messageId,
                    CorrelationId = message.Id?.ToString(),
                    Type = eventName,
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
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
            if (Interlocked.Exchange(ref this.disposed, 1) == 1)
            {
                return;
            }

            try
            {
                this.publishLock.Wait();
                try
                {
                    if (this.publisherChannel is not null)
                    {
                        this.publisherChannel.Dispose();
                    }
                }
                finally
                {
                    this.publishLock.Release();
                }
            }
            finally
            {
                this.publishLock.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 1)
            {
                return;
            }

            try
            {
                await this.publishLock.WaitAsync();
                try
                {
                    if (this.publisherChannel is not null)
                    {
                        try
                        {
                            await this.publisherChannel.CloseAsync();
                        }
                        catch
                        {
                        }

                        this.publisherChannel.Dispose();
                    }
                }
                finally
                {
                    this.publishLock.Release();
                }
            }
            finally
            {
                this.publishLock.Dispose();
            }
        }
    }
}
