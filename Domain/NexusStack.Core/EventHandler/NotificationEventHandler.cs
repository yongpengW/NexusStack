using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusStack.Core.EventData;
using NexusStack.Core.SignalR;
using NexusStack.Core.Services.AsyncTasks;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure.Enums;
using NexusStack.RabbitMQ.EventBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace NexusStack.Core.EventHandler
{
    public class NotificationEventHandler : IEventHandler<NotificationEventData>
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public NotificationEventHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task HandleAsync(NotificationEventData @event)
        {
            if (@event.TaskCode != "Notification")
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificationEventHandler>>();
            var asyncTaskService = scope.ServiceProvider.GetRequiredService<IAsyncTaskService>();

            var task = await asyncTaskService.GetAsync(x => x.Id == @event.TaskId);

            if (task is null || !(task.State == AsyncTaskState.Pending || task.State == AsyncTaskState.Retry))
            {
                logger.LogInformation($"AsyncTaskEvent 任务[{task?.Id}] 已处理完成或正在处理中");
                return;
            }

            try
            {
                task.State = AsyncTaskState.InProgress;
                await asyncTaskService.UpdateAsync(task);

                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

                var rawData = !string.IsNullOrWhiteSpace(@event.Data) ? @event.Data : task.Data;
                if (string.IsNullOrWhiteSpace(rawData))
                {
                    throw new InvalidOperationException("Notification 数据为空");
                }

                var relayMessage = JsonSerializer.Deserialize<NotificationRelayMessage>(rawData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (relayMessage?.Target is null)
                {
                    throw new InvalidOperationException("Notification 数据格式错误：缺少 target");
                }

                var eventName = string.IsNullOrWhiteSpace(relayMessage.Event) ? "Notification" : relayMessage.Event.Trim();
                var targetType = relayMessage.Target.Type?.Trim()?.ToLowerInvariant();

                object payload = relayMessage.Payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null
                    ? relayMessage
                    : relayMessage.Payload;

                if (string.IsNullOrWhiteSpace(targetType) || targetType == "user")
                {
                    var userId = relayMessage.Target.UserId?.Trim();
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        throw new InvalidOperationException("Notification 数据格式错误：target.type=user 时必须提供 target.userId");
                    }

                    // 默认按“内置用户组”推送：兼容多连接/多端；若 userId 非数字则回退到 Clients.User(userId)
                    if (long.TryParse(userId, out var userIdLong) && userIdLong > 0)
                    {
                        await hubContext.Clients.Group(NotificationGroupNames.User(userIdLong)).SendAsync(eventName, payload);
                    }
                    else
                    {
                        await hubContext.Clients.User(userId).SendAsync(eventName, payload);
                    }
                }
                else if (targetType == "group")
                {
                    var group = relayMessage.Target.Group?.Trim();
                    if (string.IsNullOrWhiteSpace(group))
                    {
                        throw new InvalidOperationException("Notification 数据格式错误：target.type=group 时必须提供 target.group");
                    }

                    await hubContext.Clients.Group(group).SendAsync(eventName, payload);
                }
                else if (targetType == "all")
                {
                    await hubContext.Clients.All.SendAsync(eventName, payload);
                }
                else
                {
                    throw new InvalidOperationException($"Notification 数据格式错误：不支持的 target.type={relayMessage.Target.Type}");
                }



                task.State = AsyncTaskState.Completed;
                task.Result = $"成功";

                await asyncTaskService.UpdateAsync(task);
            }
            catch (Exception ex)
            {
                task.State = AsyncTaskState.Fail;
                task.ErrorMessage = ex.Message;
                await asyncTaskService.UpdateAsync(task);
                logger.LogError(ex, $"AsyncTaskEvent 任务[{task.Id}] 处理失败");
            }
        }
    }
}
