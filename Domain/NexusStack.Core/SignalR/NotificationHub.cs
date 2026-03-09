using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NexusStack.Infrastructure.Attributes;

namespace NexusStack.Core.SignalR
{
    [Authorize(AuthenticationSchemes = "Authorization-SignalR-Token")]
    [SignalRHub("/hubs/notification")]
    public class NotificationHub : Hub
    {
    }
}

