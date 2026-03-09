using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NexusStack.Infrastructure.Attributes;
using System.Security.Claims;

namespace NexusStack.Core.SignalR
{
    [Authorize(AuthenticationSchemes = "Authorization-SignalR-Token")]
    [SignalRHub("/hubs/notification")]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userIdStr = Context.User?.FindFirstValue(CoreClaimTypes.UserId)
                ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (long.TryParse(userIdStr, out var userId) && userId > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, NotificationGroupNames.User(userId));
            }

            var platformTypeStr = Context.User?.FindFirstValue(CoreClaimTypes.PlatFormType);
            if (int.TryParse(platformTypeStr, out var platformType))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, NotificationGroupNames.Platform(platformType));
            }

            await base.OnConnectedAsync();
        }
    }
}

