using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusStack.RabbitMQ.EventBus;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
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
        private int delayedExchangeDeclared;
        private readonly ConcurrentDictionary<string, byte> declaredDelayQueues = new();

        public EventPublisher(IConnection connection, ILogger<EventPublisher> logger, IOptions<RabbitOptions> options)
        {
            this.connection = connection;
            this.logger = logger;
            this.options = options.Value;
            this.publisherChannel = CreateChannelAsync().GetAwaiter().GetResult();

            this.publisherChannel.BasicReturnAsync += (_, args) =>
            {
                var returnedBody = Encoding.UTF8.GetString(args.Body.ToArray());
                this.logger.LogError($"消息路由失败并被退回。Exchange:{args.Exchange}, RoutingKey:{args.RoutingKey}, ReplyCode:{args.ReplyCode}, ReplyText:{args.ReplyText}, Body:{returnedBody}");
                return Task.CompletedTask;
            };
        }

        public Task PublishAsync<TEvent>(TEvent message) where TEvent : IEvent
        {
            return PublishInternalAsync(message);
        }

        public Task PublishDelayedAsync<TEvent>(TEvent message, DelayTier delayTier) where TEvent : IEvent
        {
            return PublishDelayedInternalAsync(message, delayTier);
        }

        private async Task PublishDelayedInternalAsync<TEvent>(TEvent message, DelayTier delayTier) where TEvent : IEvent
        {
            if (Interlocked.CompareExchange(ref this.disposed, 0, 0) == 1)
            {
                throw new ObjectDisposedException(nameof(EventPublisher));
            }

            var (tierKey, tierMs) = GetTierFromEnum(delayTier);
            var eventName = message.GetType().FullName;

            await this.publishLock.WaitAsync();
            try
            {
                if (Interlocked.CompareExchange(ref this.disposed, 0, 0) == 1)
                {
                    throw new ObjectDisposedException(nameof(EventPublisher));
                }

                await EnsureDelayedExchangeAndQueueAsync(tierKey, tierMs, eventName);

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

                var delayedExchange = string.IsNullOrWhiteSpace(this.options.DelayedExchangeName)
                    ? this.options.ExchangeName + ".delayed"
                    : this.options.DelayedExchangeName;
                var routingKey = $"{tierKey}.{SanitizeEventTypeName(eventName)}";

                await this.publisherChannel.BasicPublishAsync(
                    exchange: delayedExchange,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: properties,
                    body: Encoding.UTF8.GetBytes(body));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "发布 RabbitMQ 延迟消息失败");
                throw;
            }
            finally
            {
                this.publishLock.Release();
            }
        }

        private static (string TierKey, int TierMs) GetTierFromEnum(DelayTier delayTier)
        {
            var minutes = (int)delayTier;
            var tierKey = minutes + "m";
            var tierMs = minutes * 60 * 1000;
            return (tierKey, tierMs);
        }

        private static string SanitizeEventTypeName(string eventTypeName)
        {
            if (string.IsNullOrEmpty(eventTypeName)) return string.Empty;
            return eventTypeName.Replace(' ', '_').Replace(",", "_").Replace("`", "_");
        }

        private async Task EnsureDelayedExchangeAndQueueAsync(string tierKey, int tierMs, string eventTypeName)
        {
            var delayedExchange = this.options.DelayedExchangeName ?? (this.options.ExchangeName + ".delayed");

            if (Interlocked.CompareExchange(ref this.delayedExchangeDeclared, 1, 0) == 0)
            {
                await this.publisherChannel.ExchangeDeclareAsync(
                    exchange: delayedExchange,
                    type: ExchangeType.Direct,
                    durable: true);
            }

            var sanitized = SanitizeEventTypeName(eventTypeName);
            var queueKey = $"{tierKey}:{sanitized}";
            if (this.declaredDelayQueues.ContainsKey(queueKey))
            {
                return;
            }

            var queueName = $"delay.{tierKey}.{sanitized}";
            await this.publisherChannel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    ["x-message-ttl"] = tierMs,
                    ["x-dead-letter-exchange"] = this.options.ExchangeName,
                    ["x-dead-letter-routing-key"] = eventTypeName
                });

            var routingKey = $"{tierKey}.{sanitized}";
            await this.publisherChannel.QueueBindAsync(
                queue: queueName,
                exchange: delayedExchange,
                routingKey: routingKey);

            this.declaredDelayQueues.TryAdd(queueKey, 0);
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
