using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusStack.Redis;
using NexusStack.RabbitMQ.EventBus;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NexusStack.RabbitMQ
{
    /// <summary>
    /// RabbitMQ事件订阅者
    /// </summary>
    public class EventSubscriber : IEventSubscriber
    {
        private readonly ILogger<EventSubscriber> logger;
        private readonly IConnection connection;
        private readonly IRedisService redisService;
        private readonly ConcurrentBag<Type> eventTypes;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ConcurrentDictionary<string, List<Type>> EventHandlerFactories;
        private readonly ConcurrentDictionary<string, string> consumerQueueMappings = new();
        private readonly RabbitOptions options;
        private readonly SemaphoreSlim retryPublishLock = new(1, 1);

        private IChannel consumerChannel;
        private IChannel retryPublishChannel;

        public EventSubscriber(ILogger<EventSubscriber> logger,
            IConnection connection,
            IRedisService redisService,
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitOptions> options)
        {
            this.options = options.Value;
            this.logger = logger;
            this.connection = connection;
            this.redisService = redisService;
            this.scopeFactory = scopeFactory;
            this.eventTypes = new ConcurrentBag<Type>();
            this.EventHandlerFactories = new ConcurrentDictionary<string, List<Type>>();
            this.consumerChannel = CreateConsumerChannelAsync().GetAwaiter().GetResult();
            this.retryPublishChannel = CreateRetryPublishChannelAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            this.logger.LogInformation($"IEventSubscriber Dispose");

            try
            {
                if (this.consumerChannel is not null)
                {
                    this.consumerChannel.CloseAsync().GetAwaiter().GetResult();
                    this.consumerChannel.Dispose();
                }

                if (this.retryPublishChannel is not null)
                {
                    this.retryPublishChannel.CloseAsync().GetAwaiter().GetResult();
                    this.retryPublishChannel.Dispose();
                }
            }
            finally
            {
                this.retryPublishLock.Dispose();
            }
        }

        public Task SubscribeAsync(Type eventType, Type eventHandlerType)
        {
            return SubscribeInternalAsync(eventType, eventHandlerType);
        }

        private async Task SubscribeInternalAsync(Type eventType, Type eventHandlerType)
        {
            var eventName = eventType.FullName;
            var eventHandlerName = eventHandlerType.FullName;
            var queueName = $"{eventHandlerName}";

            var dlxName = $"{this.options.ExchangeName}.dlx";
            var deadQueueName = $"{queueName}.dlq";
            var retryExchangeName = $"{this.options.ExchangeName}.retry";
            var retryQueueName = $"{queueName}.retry";

            await this.consumerChannel.ExchangeDeclareAsync(
                exchange: dlxName,
                type: ExchangeType.Direct,
                durable: true);

            await this.consumerChannel.ExchangeDeclareAsync(
                exchange: retryExchangeName,
                type: ExchangeType.Direct,
                durable: true);

            await this.consumerChannel.QueueDeclareAsync(
                queue: deadQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            await this.consumerChannel.QueueBindAsync(
                queue: deadQueueName,
                exchange: dlxName,
                routingKey: queueName);

            await this.consumerChannel.QueueDeclareAsync(
                queue: retryQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    ["x-message-ttl"] = this.options.RetryDelayMilliseconds,
                    ["x-dead-letter-exchange"] = this.options.ExchangeName,
                    ["x-dead-letter-routing-key"] = eventName
                });

            await this.consumerChannel.QueueBindAsync(
                queue: retryQueueName,
                exchange: retryExchangeName,
                routingKey: queueName);

            await this.consumerChannel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = dlxName,
                    ["x-dead-letter-routing-key"] = queueName
                });

            var consumer = new AsyncEventingBasicConsumer(this.consumerChannel);
            consumer.ReceivedAsync += OnConsumerMessageReceived;

            var consumerTag = await this.consumerChannel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            this.consumerQueueMappings[consumerTag] = queueName;

            if (!this.eventTypes.Where(item => item.FullName == eventName).Any())
            {
                this.eventTypes.Add(eventType);
            }

            this.EventHandlerFactories.AddOrUpdate(eventName, new List<Type> { eventHandlerType }, (key, list) =>
            {
                if (!list.Contains(eventHandlerType))
                {
                    list.Add(eventHandlerType);
                }
                return list;
            });

            await this.consumerChannel.QueueBindAsync(
                queue: queueName,
                exchange: this.options.ExchangeName,
                routingKey: eventName);
        }

        private async Task<IChannel> CreateConsumerChannelAsync()
        {
            var channel = await this.connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(
                exchange: this.options.ExchangeName,
                type: ExchangeType.Direct,
                durable: true);

            var prefetchCount = this.options.ConsumerDispatchConcurrency == 0 ? (ushort)10 : this.options.ConsumerDispatchConcurrency;
            await channel.BasicQosAsync(0, prefetchCount, false);

            return channel;
        }

        private async Task<IChannel> CreateRetryPublishChannelAsync()
        {
            var channel = await this.connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(
                exchange: this.options.ExchangeName,
                type: ExchangeType.Direct,
                durable: true);

            return channel;
        }

        private async Task OnConsumerMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            this.logger.LogInformation($"Message Received: {eventName} => {message}");

            var idempotencyKey = string.Empty;

            try
            {
                var idempotencyResult = await TryAcquireIdempotencyAsync(eventArgs, message);
                if (!string.IsNullOrEmpty(idempotencyResult) && idempotencyResult.StartsWith("DUPLICATE|"))
                {
                    this.logger.LogInformation($"检测到重复消息，直接确认并跳过处理。RoutingKey:{eventName}");
                    await this.consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, false);
                    return;
                }

                idempotencyKey = idempotencyResult.Replace("ACQUIRED|", string.Empty);

                if (await ProcessEvent(eventName, message))
                {
                    // 处理成功，确认消息
                    await this.consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, false);
                    return;
                }

                await HandleFailureAsync(eventArgs, idempotencyKey);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "消息处理过程中发生异常");
                await HandleFailureAsync(eventArgs, idempotencyKey);
            }
        }

        private async Task HandleFailureAsync(BasicDeliverEventArgs eventArgs, string idempotencyKey)
        {
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                await this.redisService.DeleteAsync(idempotencyKey);
            }

            var currentRetryCount = GetRetryCount(eventArgs.BasicProperties?.Headers);
            if (currentRetryCount < this.options.MaxRetryCount)
            {
                try
                {
                    await RepublishForRetryAsync(eventArgs, currentRetryCount + 1);
                    await this.consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, false);
                    return;
                }
                catch (Exception republishEx)
                {
                    this.logger.LogError(republishEx, $"重试消息发布失败，直接进入DLQ。RoutingKey:{eventArgs.RoutingKey}");
                }
            }

            // 超过最大重试次数或重试消息发布失败，拒绝消息，不重新入队，交由 DLQ 承接
            await this.consumerChannel.BasicNackAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                requeue: false);
        }

        private async Task<string> TryAcquireIdempotencyAsync(BasicDeliverEventArgs eventArgs, string message)
        {
            if (!this.options.EnableConsumerIdempotency)
            {
                return string.Empty;
            }

            var messageId = eventArgs.BasicProperties?.MessageId;
            if (string.IsNullOrWhiteSpace(messageId))
            {
                messageId = eventArgs.BasicProperties?.CorrelationId;
            }

            if (string.IsNullOrWhiteSpace(messageId))
            {
                var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(message));
                messageId = Convert.ToHexString(hashBytes);
            }

            var key = $"mq:idempotent:{eventArgs.RoutingKey}:{messageId}";
            var acquired = await this.redisService.SetAsync(
                key,
                DateTimeOffset.UtcNow.ToString("O"),
                TimeSpan.FromHours(this.options.ConsumerIdempotencyExpireHours),
                CSRedis.RedisExistence.Nx);

            return acquired ? $"ACQUIRED|{key}" : $"DUPLICATE|{key}";
        }

        private static int GetRetryCount(IDictionary<string, object>? headers)
        {
            if (headers is null || !headers.TryGetValue("x-retry-count", out var retryObj) || retryObj is null)
            {
                return 0;
            }

            try
            {
                return retryObj switch
                {
                    byte b => b,
                    sbyte sb => sb,
                    short s => s,
                    ushort us => us,
                    int i => i,
                    uint ui => (int)ui,
                    long l => (int)l,
                    ulong ul => (int)ul,
                    byte[] bytes => int.Parse(Encoding.UTF8.GetString(bytes)),
                    _ => int.Parse(retryObj.ToString() ?? "0")
                };
            }
            catch
            {
                return 0;
            }
        }

        private async Task RepublishForRetryAsync(BasicDeliverEventArgs eventArgs, int nextRetryCount)
        {
            var headers = eventArgs.BasicProperties?.Headers is null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(eventArgs.BasicProperties.Headers);

            headers["x-retry-count"] = nextRetryCount;

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = eventArgs.BasicProperties?.ContentType,
                ContentEncoding = eventArgs.BasicProperties?.ContentEncoding,
                CorrelationId = eventArgs.BasicProperties?.CorrelationId,
                MessageId = eventArgs.BasicProperties?.MessageId,
                Timestamp = eventArgs.BasicProperties?.Timestamp ?? default,
                Type = eventArgs.BasicProperties?.Type,
                AppId = eventArgs.BasicProperties?.AppId,
                Headers = headers
            };

            if (!this.consumerQueueMappings.TryGetValue(eventArgs.ConsumerTag, out var queueName))
            {
                queueName = eventArgs.RoutingKey;
            }

            var retryExchangeName = $"{this.options.ExchangeName}.retry";

            this.logger.LogWarning($"消息处理失败，准备延迟重试，第{nextRetryCount}次。RoutingKey:{eventArgs.RoutingKey}, Queue:{queueName}");

            await this.retryPublishLock.WaitAsync();
            try
            {
                await this.retryPublishChannel.BasicPublishAsync(
                    exchange: retryExchangeName,
                    routingKey: queueName,
                    mandatory: true,
                    basicProperties: properties,
                    body: eventArgs.Body);
            }
            finally
            {
                this.retryPublishLock.Release();
            }
        }

        private async Task<bool> ProcessEvent(string eventName, string message)
        {
            try
            {
                Type eventType = this.eventTypes.SingleOrDefault(item => item.FullName == eventName);

                if (eventType is null)
                {
                    this.logger.LogError($"未找到事件类型定义: {eventName}");
                    return false;
                }

                if (!this.EventHandlerFactories.TryGetValue(eventName, out var eventHandlers) || eventHandlers.Count == 0)
                {
                    this.logger.LogError($"未找到事件处理器: {eventName}");
                    return false;
                }

                var eventData = JsonSerializer.Deserialize(message, eventType, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (eventData is null)
                {
                    this.logger.LogError($"事件反序列化失败: {eventName}");
                    return false;
                }

                foreach (var eventHandler in eventHandlers)
                {
                    using var handlerScope = this.scopeFactory.CreateScope();
                    var resolvedHandler = handlerScope.ServiceProvider.GetService(eventHandler) as IEventHandler;
                    var handler = resolvedHandler ?? Activator.CreateInstance(eventHandler, this.scopeFactory) as IEventHandler;

                    if (handler is null)
                    {
                        this.logger.LogError($"无法创建事件处理器实例: {eventHandler.FullName}");
                        return false;
                    }

                    using var logScope = this.logger.BeginScope(new Dictionary<string, object>
                    {
                        ["EventBusId"] = (eventData as EventBase)?.Id ?? string.Empty,
                        ["Handler"] = handler.GetType().FullName ?? eventHandler.FullName ?? string.Empty,
                    });

                    try
                    {
                        var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                        var handleMethod = concreteType.GetMethod("HandleAsync");
                        if (handleMethod is null)
                        {
                            this.logger.LogError($"未找到 HandleAsync 方法: {eventHandler.FullName}");
                            return false;
                        }

                        this.logger.LogInformation($"开始执行 {eventName} 事件, 内容：{message}");

                        await (Task)handleMethod.Invoke(handler, new[] { eventData })!;
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogInformation($"事件处理程序处理事件时发生错误，消息内容:{message}");
                        this.logger.LogError(ex, ex.Message);
                        return false; // 处理失败
                    }
                    finally
                    {
                        this.logger.LogInformation($"事件 {eventName} 执行完成");
                    }
                }

                return true; // 处理成功
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"ProcessEvent 处理失败: {ex.Message}");
                return false; // 处理失败
            }
        }
    }
}
