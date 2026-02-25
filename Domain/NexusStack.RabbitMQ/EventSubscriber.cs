using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusStack.RabbitMQ.EventBus;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly ConcurrentBag<Type> eventTypes;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ConcurrentDictionary<string, List<Type>> EventHandlerFactories;
        private readonly RabbitOptions options;

        private IChannel consumerChannel;

        public EventSubscriber(ILogger<EventSubscriber> logger, IConnection connection, IServiceScopeFactory scopeFactory, IOptions<RabbitOptions> options)
        {
            this.options = options.Value;
            this.logger = logger;
            this.connection = connection;
            this.scopeFactory = scopeFactory;
            this.eventTypes = new ConcurrentBag<Type>();
            this.EventHandlerFactories = new ConcurrentDictionary<string, List<Type>>();
            this.consumerChannel = CreateConsumerChannelAsync().GetAwaiter().GetResult();
        }

        public async void Dispose()
        {
            this.logger.LogInformation($"IEventSubscriber Dispose");
            if (this.consumerChannel != null)
            {
                await this.consumerChannel.CloseAsync();
            }
        }

        public async void Subscribe(Type eventType, Type eventHandlerType)
        {
            var eventName = eventType.FullName;
            var eventHandlerName = eventHandlerType.FullName;
            var queueName = $"{eventHandlerName}";

            await this.consumerChannel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: true);

            var consumer = new AsyncEventingBasicConsumer(this.consumerChannel);
            consumer.ReceivedAsync += OnConsumerMessageReceived;

            await this.consumerChannel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            if (!this.eventTypes.Where(item => item.FullName == eventName).Any())
            {
                this.eventTypes.Add(eventType);
            }

            this.EventHandlerFactories.AddOrUpdate(eventName, new List<Type> { eventHandlerType }, (key, list) =>
            {
                list.Add(eventHandlerType);
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

            return channel;
        }

        private async Task OnConsumerMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            this.logger.LogInformation($"Message Received: {eventName} => {message}");

            try
            {
                if (await ProcessEvent(eventName, message))
                {
                    // 处理成功，确认消息
                    await this.consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, false);
                }
                else
                {
                    // 处理失败，拒绝消息并重新入队
                    await this.consumerChannel.BasicNackAsync(
                        deliveryTag: eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: true);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "消息处理过程中发生异常");
                // 发生异常时，拒绝消息并重新入队
                await this.consumerChannel.BasicNackAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        }

        private async Task<bool> ProcessEvent(string eventName, string message)
        {
            try
            {
                Type eventType = this.eventTypes.SingleOrDefault(item => item.FullName == eventName);

                if (this.EventHandlerFactories.TryGetValue(eventName, out var eventHandlers) && eventHandlers.Count > 0)
                {
                    foreach (var eventHandler in eventHandlers)
                    {
                        // 通过反射创建事件处理程序实例，并传递IServiceScopeFactory参数
                        var handler = (IEventHandler)Activator.CreateInstance(eventHandler, scopeFactory);

                        var eventData = JsonSerializer.Deserialize(message, eventType, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        using var scope = this.logger.BeginScope(new Dictionary<string, object>
                        {
                            ["EventBusId"] = ((EventBase)eventData).Id,
                            ["Handler"] = handler.GetType().FullName,
                        });

                        try
                        {
                            // 创建一个异步操作点，允许异步方法在执行时暂时释放线程
                            await Task.Yield();

                            var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                            this.logger.LogInformation($"开始执行 {eventName} 事件, 内容：{message}");

                            await (Task)concreteType.GetMethod("HandleAsync").Invoke(handler, new object[] { eventData });
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
                            scope.Dispose();
                        }
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
