using NexusStack.Core.Entities.AsyncTasks;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Core.SignalR;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Options;
using System.Text.Json;

namespace NexusStack.Core.Services.SignalR
{
    public class NotificationRelayService(IAsyncTaskService asyncTaskService) : INotificationRelayService, IScopedDependency
    {
        public Task<AsyncTask> NotifyToUserAsync(long userId, object payload, string eventName = "Notification")
        {
            var message = new NotificationRelayMessage
            {
                Target = new NotificationTarget
                {
                    Type = "user",
                    UserId = userId.ToString()
                },
                Event = eventName,
                Payload = JsonSerializer.SerializeToElement(payload, JsonOptions.Default)
            };

            return asyncTaskService.CreateTaskAsync(message, "Notification");
        }

        public Task<AsyncTask> NotifyToGroupAsync(string group, object payload, string eventName = "Notification")
        {
            var message = new NotificationRelayMessage
            {
                Target = new NotificationTarget
                {
                    Type = "group",
                    Group = group
                },
                Event = eventName,
                Payload = JsonSerializer.SerializeToElement(payload, JsonOptions.Default)
            };

            return asyncTaskService.CreateTaskAsync(message, "Notification");
        }

        public Task<AsyncTask> NotifyToAllAsync(object payload, string eventName = "Notification")
        {
            var message = new NotificationRelayMessage
            {
                Target = new NotificationTarget
                {
                    Type = "all"
                },
                Event = eventName,
                Payload = JsonSerializer.SerializeToElement(payload, JsonOptions.Default)
            };

            return asyncTaskService.CreateTaskAsync(message, "Notification");
        }
    }
}

