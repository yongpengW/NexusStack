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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NexusStack.RabbitMQ
{
    /// <summary>
    /// RabbitMQ事件订阅者
    /// </summary>
    public class EventSubscriber : IEventSubscriber, IAsyncDisposable
    {
        private readonly ILogger<EventSubscriber> logger;
        private readonly IConnection connection;
        private readonly IRedisService redisService;
        private readonly ConcurrentBag<Type> eventTypes;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ConcurrentDictionary<string, List<Type>> EventHandlerFactories;
        private readonly ConcurrentDictionary<string, string> consumerQueueMappings = new();
        private readonly ConcurrentDictionary<string, Type> consumerHandlerMappings = new();
        private readonly ConcurrentDictionary<string, IChannel> consumerChannelMappings = new();
        private readonly ConcurrentDictionary<string, IChannel> consumerChannelsByQueue = new();
        private readonly RabbitOptions options;
        private readonly SemaphoreSlim retryPublishLock = new(1, 1);
        private readonly SemaphoreSlim subscribeLock = new(1, 1);

        private IChannel retryPublishChannel;
        private int disposed;

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
            this.retryPublishChannel = CreateRetryPublishChannelAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 1)
            {
                return;
            }

            this.logger.LogInformation($"IEventSubscriber Dispose");

            try
            {
                this.subscribeLock.Wait();
                try
                {
                    var channels = consumerChannelsByQueue.Values.Distinct().ToList();
                    foreach (var channel in channels)
                    {
                        if (channel is null)
                        {
                            continue;
                        }

                        try
                        {
                            channel.Dispose();
                        }
                        catch
                        {
                        }
                    }
                }
                finally
                {
                    this.subscribeLock.Release();
                }

                this.retryPublishLock.Wait();
                try
                {
                    if (this.retryPublishChannel is not null)
                    {
                        try
                        {
                            this.retryPublishChannel.Dispose();
                        }
                        catch
                        {
                        }
                    }
                }
                finally
                {
                    this.retryPublishLock.Release();
                }
            }
            finally
            {
                this.subscribeLock.Dispose();
                this.retryPublishLock.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref this.disposed, 1) == 1)
            {
                return;
            }

            this.logger.LogInformation($"IEventSubscriber Dispose");

            try
            {
                await this.subscribeLock.WaitAsync();
                try
                {
                    var channels = consumerChannelsByQueue.Values.Distinct().ToList();
                    foreach (var channel in channels)
                    {
                        if (channel is null)
                        {
                            continue;
                        }

                        try
                        {
                            await channel.CloseAsync();
                        }
                        catch
                        {
                        }

                        channel.Dispose();
                    }
                }
                finally
                {
                    this.subscribeLock.Release();
                }

                await this.retryPublishLock.WaitAsync();
                try
                {
                    if (this.retryPublishChannel is not null)
                    {
                        try
                        {
                            await this.retryPublishChannel.CloseAsync();
                        }
                        catch
                        {
                        }

                        this.retryPublishChannel.Dispose();
                    }
                }
                finally
                {
                    this.retryPublishLock.Release();
                }
            }
            finally
            {
                this.subscribeLock.Dispose();
                this.retryPublishLock.Dispose();
            }
        }

        public Task SubscribeAsync(Type eventType, Type eventHandlerType)
        {
            return SubscribeInternalAsync(eventType, eventHandlerType);
        }

        private async Task SubscribeInternalAsync(Type eventType, Type eventHandlerType)
        {
            if (Interlocked.CompareExchange(ref this.disposed, 0, 0) == 1)
            {
                throw new ObjectDisposedException(nameof(EventSubscriber));
            }

            var eventName = eventType.FullName;
            var eventHandlerName = eventHandlerType.FullName;
            var queueName = $"{eventHandlerName}";

            await this.subscribeLock.WaitAsync();
            try
            {
                // 锁内再次检查 disposed
                if (Interlocked.CompareExchange(ref this.disposed, 0, 0) == 1)
                {
                    throw new ObjectDisposedException(nameof(EventSubscriber));
                }

                if (this.consumerChannelsByQueue.ContainsKey(queueName))
                {
                    this.logger.LogWarning($"队列已订阅，忽略重复订阅。Queue:{queueName}");
                    return;
                }

                var consumerChannel = await CreateConsumerChannelAsync();
                try
                {
                    var dlxName = $"{this.options.ExchangeName}.dlx";
                    var deadQueueName = $"{queueName}.dlq";
                    var retryExchangeName = $"{this.options.ExchangeName}.retry";
                    var retryQueueName = $"{queueName}.retry";

                    await consumerChannel.ExchangeDeclareAsync(
                        exchange: dlxName,
                        type: ExchangeType.Direct,
                        durable: true);

                    await consumerChannel.ExchangeDeclareAsync(
                        exchange: retryExchangeName,
                        type: ExchangeType.Direct,
                        durable: true);

                    await consumerChannel.QueueDeclareAsync(
                        queue: deadQueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: new Dictionary<string, object>
                        {
                            ["x-message-ttl"] = (int)TimeSpan.FromDays(7).TotalMilliseconds
                        });

                    await consumerChannel.QueueBindAsync(
                        queue: deadQueueName,
                        exchange: dlxName,
                        routingKey: queueName);

                    await consumerChannel.QueueDeclareAsync(
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

                    await consumerChannel.QueueBindAsync(
                        queue: retryQueueName,
                        exchange: retryExchangeName,
                        routingKey: queueName);

                    await consumerChannel.QueueDeclareAsync(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: new Dictionary<string, object>
                        {
                            ["x-dead-letter-exchange"] = dlxName,
                            ["x-dead-letter-routing-key"] = queueName
                        });

                    var consumer = new AsyncEventingBasicConsumer(consumerChannel);
                    consumer.ReceivedAsync += OnConsumerMessageReceived;
                    consumer.UnregisteredAsync += OnConsumerUnregisteredAsync;
                    consumer.ShutdownAsync += OnConsumerShutdownAsync;

                    var consumerTag = await consumerChannel.BasicConsumeAsync(
                        queue: queueName,
                        autoAck: false,
                        consumer: consumer);

                    this.consumerQueueMappings[consumerTag] = queueName;
                    this.consumerHandlerMappings[consumerTag] = eventHandlerType;
                    this.consumerChannelMappings[consumerTag] = consumerChannel;
                    this.consumerChannelsByQueue[queueName] = consumerChannel;

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

                    await consumerChannel.QueueBindAsync(
                        queue: queueName,
                        exchange: this.options.ExchangeName,
                        routingKey: eventName);
                }
                catch
                {
                    try
                    {
                        await consumerChannel.CloseAsync();
                    }
                    catch
                    {
                    }

                    consumerChannel.Dispose();
                    throw;
                }
            }
            finally
            {
                this.subscribeLock.Release();
            }
        }

        private Task OnConsumerUnregisteredAsync(object sender, ConsumerEventArgs eventArgs)
        {
            foreach (var consumerTag in eventArgs.ConsumerTags)
            {
                RemoveConsumerMappings(consumerTag);
            }

            return Task.CompletedTask;
        }

        private Task OnConsumerShutdownAsync(object sender, ShutdownEventArgs eventArgs)
        {
            if (sender is IAsyncBasicConsumer asyncConsumer)
            {
                var channel = asyncConsumer.Channel;
                var tags = consumerChannelMappings
                    .Where(x => ReferenceEquals(x.Value, channel))
                    .Select(x => x.Key)
                    .ToList();

                foreach (var tag in tags)
                {
                    RemoveConsumerMappings(tag);
                }

                var queues = consumerChannelsByQueue
                    .Where(x => ReferenceEquals(x.Value, channel))
                    .Select(x => x.Key)
                    .ToList();

                foreach (var queue in queues)
                {
                    consumerChannelsByQueue.TryRemove(queue, out _);
                }
            }

            return Task.CompletedTask;
        }

        private void RemoveConsumerMappings(string consumerTag)
        {
            consumerChannelMappings.TryRemove(consumerTag, out _);
            consumerHandlerMappings.TryRemove(consumerTag, out _);

            if (consumerQueueMappings.TryRemove(consumerTag, out var queueName)
                && !consumerQueueMappings.Values.Contains(queueName))
            {
                consumerChannelsByQueue.TryRemove(queueName, out _);
            }
        }

        private async Task<IChannel> CreateConsumerChannelAsync()
        {
            var channel = await this.connection.CreateChannelAsync(enablePublisherConfirmations: false);
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

            if (!consumerChannelMappings.TryGetValue(eventArgs.ConsumerTag, out var consumerChannel))
            {
                this.logger.LogError($"未找到 consumerTag 对应的消费通道，降级执行Nack并丢入DLQ。ConsumerTag:{eventArgs.ConsumerTag}");

                if (sender is IAsyncBasicConsumer asyncConsumer)
                {
                    try
                    {
                        await asyncConsumer.Channel.BasicNackAsync(eventArgs.DeliveryTag, false, false);
                    }
                    catch (Exception channelEx)
                    {
                        this.logger.LogError(channelEx, "consumerTag 缺失时降级Nack失败");
                    }
                }

                return;
            }

            if (!consumerHandlerMappings.TryGetValue(eventArgs.ConsumerTag, out var handlerType))
            {
                this.logger.LogError($"未找到 consumerTag 对应的处理器类型，降级执行Nack并丢入DLQ。ConsumerTag:{eventArgs.ConsumerTag}");

                try
                {
                    await consumerChannel.BasicNackAsync(eventArgs.DeliveryTag, false, false);
                }
                catch (Exception channelEx)
                {
                    this.logger.LogError(channelEx, "handler 类型缺失时降级Nack失败");
                }

                return;
            }

            var idempotencyKey = string.Empty;
            var processedSuccessfully = false;

            try
            {
                var idempotencyResult = await TryAcquireIdempotencyAsync(eventArgs, message);
                if (!string.IsNullOrEmpty(idempotencyResult) && idempotencyResult.StartsWith("DUPLICATE|"))
                {
                    this.logger.LogInformation($"检测到重复消息，直接确认并跳过处理。RoutingKey:{eventName}");
                    await consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, false);
                    return;
                }

                idempotencyKey = idempotencyResult.Replace("ACQUIRED|", string.Empty);

                if (await ProcessEvent(eventName, message, handlerType))
                {
                    processedSuccessfully = true;
                    // 处理成功，确认消息
                    await consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, false);
                    return;
                }

                await HandleFailureAsync(eventArgs, idempotencyKey, consumerChannel);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "消息处理过程中发生异常");

                // 业务已成功但 ACK 失败时，保留幂等键，等待消息重投后走幂等短路
                if (processedSuccessfully)
                {
                    this.logger.LogWarning($"消息业务已处理成功但ACK失败，保留幂等键等待重投后去重。RoutingKey:{eventArgs.RoutingKey}");
                    return;
                }

                await HandleFailureAsync(eventArgs, idempotencyKey, consumerChannel);
            }
        }

        private async Task HandleFailureAsync(BasicDeliverEventArgs eventArgs, string idempotencyKey, IChannel consumerChannel)
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
                    await consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, false);
                    return;
                }
                catch (Exception republishEx)
                {
                    this.logger.LogError(republishEx, $"重试消息发布失败，直接进入DLQ。RoutingKey:{eventArgs.RoutingKey}");
                }
            }

            // 超过最大重试次数或重试消息发布失败，拒绝消息，不重新入队，交由 DLQ 承接
            await consumerChannel.BasicNackAsync(
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

            var queueDimension = this.consumerQueueMappings.TryGetValue(eventArgs.ConsumerTag, out var queueName)
                ? queueName
                : eventArgs.RoutingKey;

            var key = $"mq:idempotent:{queueDimension}:{messageId}";
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

        private async Task<bool> ProcessEvent(string eventName, string message, Type eventHandlerType)
        {
            try
            {
                Type eventType = this.eventTypes.SingleOrDefault(item => item.FullName == eventName);

                if (eventType is null)
                {
                    this.logger.LogError($"未找到事件类型定义: {eventName}");
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

                using var handlerScope = this.scopeFactory.CreateScope();
                var resolvedHandler = handlerScope.ServiceProvider.GetService(eventHandlerType) as IEventHandler;
                var handler = resolvedHandler ?? Activator.CreateInstance(eventHandlerType, this.scopeFactory) as IEventHandler;

                if (handler is null)
                {
                    this.logger.LogError($"无法创建事件处理器实例: {eventHandlerType.FullName}");
                    return false;
                }

                using var logScope = this.logger.BeginScope(new Dictionary<string, object>
                {
                    ["EventBusId"] = (eventData as EventBase)?.Id ?? string.Empty,
                    ["Handler"] = handler.GetType().FullName ?? eventHandlerType.FullName ?? string.Empty,
                });

                try
                {
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    var handleMethod = concreteType.GetMethod("HandleAsync");
                    if (handleMethod is null)
                    {
                        this.logger.LogError($"未找到 HandleAsync 方法: {eventHandlerType.FullName}");
                        return false;
                    }

                    this.logger.LogInformation($"开始执行 {eventName} 事件, Handler: {eventHandlerType.FullName}, 内容：{message}");

                    await (Task)handleMethod.Invoke(handler, new[] { eventData })!;
                }
                catch (Exception ex)
                {
                    this.logger.LogInformation($"事件处理程序处理事件时发生错误，Handler: {eventHandlerType.FullName}, 消息内容:{message}");
                    this.logger.LogError(ex, ex.Message);
                    return false;
                }
                finally
                {
                    this.logger.LogInformation($"事件 {eventName} Handler {eventHandlerType.FullName} 执行完成");
                }

                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"ProcessEvent 处理失败: {ex.Message}");
                return false;
            }
        }
    }
}
