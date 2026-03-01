using Microsoft.EntityFrameworkCore;
using NexusStack.Core.Dtos.Users;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.DbContexts;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Constants;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Utils;
using NexusStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NexusStack.Core.Services.Users
{
    /// <summary>
    /// 用户上下文缓存服务：按 (UserId, PlatformType) 缓存 Roles、Regions 等，供鉴权后从 Redis 读取。
    /// </summary>
    public class UserContextCacheService(
        MainContext dbContext,
        IRedisService redisService,
        IUserRoleService userRoleService) : IUserContextCacheService, IScopedDependency
    {
        private static readonly TimeSpan DefaultExpire = TimeSpan.FromHours(10);

        private static string CacheKey(long userId, PlatformType platformType) =>
            CoreRedisConstants.UserContext.Format(userId, (int)platformType);

        public async Task<UserContextCacheDto> GetOrSetAsync(long userId, PlatformType platformType, TimeSpan? expire = null, CancellationToken cancellationToken = default)
        {
            var key = CacheKey(userId, platformType);
            var cached = await redisService.GetAsync<UserContextCacheDto>(key);
            if (cached != null)
                return cached;

            var context = await BuildFromDbAsync(userId, platformType, cancellationToken);
            var ttl = expire ?? DefaultExpire;
            await redisService.SetAsync(key, context, ttl);
            return context;
        }

        public async Task SetAsync(long userId, PlatformType platformType, UserContextCacheDto context, TimeSpan? expire = null, CancellationToken cancellationToken = default)
        {
            var key = CacheKey(userId, platformType);
            var ttl = expire ?? DefaultExpire;
            await redisService.SetAsync(key, context, ttl);
        }

        public async Task InvalidateAsync(long userId, PlatformType? platformType = null, CancellationToken cancellationToken = default)
        {
            if (platformType.HasValue)
            {
                await redisService.DeleteAsync(CacheKey(userId, platformType.Value));
                return;
            }

            foreach (var p in new[] { PlatformType.Admin, PlatformType.Pc, PlatformType.Mini, PlatformType.Android })
                await redisService.DeleteAsync(CacheKey(userId, p));
        }

        private async Task<UserContextCacheDto> BuildFromDbAsync(long userId, PlatformType platformType, CancellationToken cancellationToken)
        {
            var user = await dbContext.Set<User>()
                .Where(u => u.Id == userId)
                .Select(u => new { u.UserName, u.Email })
                .FirstOrDefaultAsync(cancellationToken);

            var roleIds = (await userRoleService.GetUserRoles(userId, platformType))
                .Select(ur => ur.RoleId)
                .Distinct()
                .ToList();

            var regionIds = await dbContext.Set<UserDepartment>()
                .Where(ud => ud.UserId == userId)
                .Select(ud => ud.DepartmentId)
                .ToListAsync(cancellationToken);

            return new UserContextCacheDto
            {
                UserName = user?.UserName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                RoleIds = roleIds,
                RegionIds = regionIds
            };
        }
    }
}
