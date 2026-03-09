using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusStack.Core.EventData;
using NexusStack.Core.Services.AsyncTasks;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure.Enums;
using NexusStack.RabbitMQ.EventBus;
using System;
using System.Collections.Generic;
using System.Text;

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

                // Todo : 使用SignalR服务执行发送具体的通知逻辑
                //
                //



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
