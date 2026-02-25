using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusStack.Core.EventData;
using NexusStack.Core.Services.SystemManagement;
using NexusStack.RabbitMQ.EventBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.EventHandler
{
    public class OperationLogEventHandler(IServiceScopeFactory scopeFactory) : IEventHandler<OperationLogEventData>
    {
        public async Task HandleAsync(OperationLogEventData @event)
        {
            using var scope = scopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<OperationLogEventHandler>>();
            logger.LogInformation("OperationLogEventHandler");
            var operationLogService = scope.ServiceProvider.GetRequiredService<IOperationLogService>();
            await operationLogService.LogAsync(@event.Code, @event.Content, @event.Json, @event.IpAddress, @event.UserAgent, @event.LogType, @event.Method, @event.UserId);
        }
    }
}
