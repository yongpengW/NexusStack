using NexusStack.Core.Dtos.Users;
using NexusStack.Infrastructure.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace NexusStack.Core.Services.Interfaces
{
    /// <summary>
    /// 用户上下文缓存服务：按 (UserId, PlatformType) 缓存 Roles、Regions 等，供鉴权后从 Redis 读取，不参与 Token 存储。
    /// </summary>
    public interface IUserContextCacheService
    {
        /// <summary>
        /// 获取或构建用户上下文：先读 Redis，未命中则从 DB 构建并写入 Redis。
        /// </summary>
        Task<UserContextCacheDto> GetOrSetAsync(long userId, PlatformType platformType, TimeSpan? expire = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 写入用户上下文缓存（如登录时预热）。
        /// </summary>
        Task SetAsync(long userId, PlatformType platformType, UserContextCacheDto context, TimeSpan? expire = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 使用户上下文缓存失效（如用户角色/部门变更时调用）。传入 null 表示该用户所有平台均失效。
        /// </summary>
        Task InvalidateAsync(long userId, PlatformType? platformType = null, CancellationToken cancellationToken = default);
    }
}
