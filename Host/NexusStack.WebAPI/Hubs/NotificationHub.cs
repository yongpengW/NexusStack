using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NexusStack.Core;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Attributes;
using System.Text.RegularExpressions;

namespace NexusStack.WebAPI.Hubs
{
    /// <summary>
    /// 通知Hub - 处理实时通知推送
    /// </summary>
    [Authorize]
    [SignalRHub("/hubs/notification")]
    public class NotificationHub : Hub, IScopedDependency
    {
        /// <summary>
        /// 连接时调用
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            Console.WriteLine($"NotificationHub: User {userId} connected with connection {connectionId}");

            // 如果UserIdentifier为空，尝试从Claims中获取
            if (string.IsNullOrEmpty(userId))
            {
                userId = Context.User?.FindFirst(CoreClaimTypes.UserId)?.Value;

                Console.WriteLine($"NotificationHub: Extracted userId from claims: {userId}");
            }

            // 自动加入用户组
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(connectionId, $"user_{userId}");
                Console.WriteLine($"NotificationHub: User {userId} automatically joined user group: user_{userId}");

                // 向客户端发送连接成功的确认消息
                await Clients.Caller.SendAsync("ConnectionStatus", new
                {
                    Connected = true,
                    UserId = userId,
                    ConnectionId = connectionId,
                    UserGroup = $"user_{userId}",
                    Timestamp = DateTimeOffset.Now,
                    ServerTime = DateTime.Now
                });
            }
            else
            {
                Console.WriteLine($"NotificationHub: Warning - No user identifier found for connection {connectionId}");

                // 发送警告给客户端
                await Clients.Caller.SendAsync("ConnectionWarning", new
                {
                    Message = "User identifier not found. User-specific notifications may not work.",
                    ConnectionId = connectionId,
                    Timestamp = DateTimeOffset.Now
                });
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 断开连接时调用
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            // 如果UserIdentifier为空，尝试从Claims中获取
            if (string.IsNullOrEmpty(userId))
            {
                userId = Context.User?.FindFirst(CoreClaimTypes.UserId)?.Value;
            }

            Console.WriteLine($"NotificationHub: User {userId} disconnected. Connection {connectionId}");

            if (exception != null)
            {
                Console.WriteLine($"NotificationHub: Disconnect reason: {exception.Message}");
            }

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(connectionId, $"user_{userId}");
                Console.WriteLine($"NotificationHub: User {userId} removed from user group: user_{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 加入店铺组
        /// </summary>
        /// <param name="shopId">店铺ID</param>
        /// <returns></returns>
        public async Task JoinShopGroup(string shopId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"shop_{shopId}");
            await Clients.Caller.SendAsync("JoinedGroup", $"shop_{shopId}");
            Console.WriteLine($"NotificationHub: User {Context.UserIdentifier} joined shop group: shop_{shopId}");
        }

        /// <summary>
        /// 离开店铺组
        /// </summary>
        /// <param name="shopId">店铺ID</param>
        /// <returns></returns>
        public async Task LeaveShopGroup(string shopId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"shop_{shopId}");
            await Clients.Caller.SendAsync("LeftGroup", $"shop_{shopId}");
            Console.WriteLine($"NotificationHub: User {Context.UserIdentifier} left shop group: shop_{shopId}");
        }

        /// <summary>
        /// 加入用户组（手动调用）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public async Task JoinUserGroup(string userId)
        {
            // 获取当前用户ID
            var currentUserId = Context.User?.FindFirst(CoreClaimTypes.UserId)?.Value;

            // 验证用户只能加入自己的组或管理员可以加入任意组
            if (currentUserId == userId)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                await Clients.Caller.SendAsync("JoinedGroup", $"user_{userId}");
                Console.WriteLine($"NotificationHub: User {currentUserId} joined user group: user_{userId}");
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "You can only join your own user group");
                Console.WriteLine($"NotificationHub: User {currentUserId} tried to join user group: user_{userId} but was denied");
            }
        }

        /// <summary>
        /// 离开用户组
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            await Clients.Caller.SendAsync("LeftGroup", $"user_{userId}");
            Console.WriteLine($"NotificationHub: User {Context.UserIdentifier} left user group: user_{userId}");
        }

        /// <summary>
        /// 订阅用户通知
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public async Task SubscribeToUserNotifications(string userId)
        {
            await JoinUserGroup(userId);
        }

        /// <summary>
        /// 取消订阅用户通知
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public async Task UnsubscribeFromUserNotifications(string userId)
        {
            await LeaveUserGroup(userId);
        }

        /// <summary>
        /// 获取用户状态
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public async Task GetUserStatus(string userId)
        {
            var currentUserId = Context.User?.FindFirst(CoreClaimTypes.UserId)?.Value;

            var status = new
            {
                UserId = userId,
                CurrentUserId = currentUserId,
                IsOnline = true, // 这里可以实现真实的在线状态检测
                ConnectionId = Context.ConnectionId,
                ConnectedAt = DateTime.Now,
                Groups = new[] { $"user_{userId}" }, // 可以返回用户所在的组
                ClaimsCount = Context.User?.Claims?.Count() ?? 0,
                AuthenticationType = Context.User?.Identity?.AuthenticationType,
                IsAuthenticated = Context.User?.Identity?.IsAuthenticated ?? false
            };

            await Clients.Caller.SendAsync("UserStatusUpdated", status);
        }

        /// <summary>
        /// 手动测试用户通知（仅用于调试）
        /// </summary>
        /// <param name="userId">目标用户ID</param>
        /// <param name="message">测试消息</param>
        /// <returns></returns>
        public async Task SendTestUserNotification(string userId, string message)
        {
            var currentUserId = Context.User?.FindFirst(CoreClaimTypes.UserId)?.Value;

            var notification = new
            {
                Title = "Hub测试通知",
                Message = message,
                Content = message,
                Type = "info",
                Timestamp = DateTimeOffset.Now,
                UserId = userId,
                SentBy = currentUserId
            };

            Console.WriteLine($"NotificationHub: Sending test notification from {currentUserId} to user group: user_{userId}");
            await Clients.Group($"user_{userId}").SendAsync("UserNotification", notification);
            await Clients.Caller.SendAsync("TestNotificationSent", $"Test notification sent to user_{userId}");
        }

        /// <summary>
        /// 发送消息给特定组
        /// </summary>
        /// <param name="groupName">组名</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        public async Task SendMessageToGroup(string groupName, string message)
        {
            var currentUserId = Context.User?.FindFirst(CoreClaimTypes.UserId)?.Value;

            var notification = new
            {
                From = currentUserId,
                GroupName = groupName,
                Message = message,
                Timestamp = DateTimeOffset.Now
            };

            await Clients.Group(groupName).SendAsync("ReceiveMessage", notification);
            Console.WriteLine($"NotificationHub: Message sent to group {groupName} by user {currentUserId}");
        }
    }
}
