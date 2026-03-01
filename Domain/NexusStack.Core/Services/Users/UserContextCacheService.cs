using Microsoft.EntityFrameworkCore;
using NexusStack.Core.Dtos.Users;
using NexusStack.Core.Entities.SystemManagement;
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

        // v2：权限 Key 格式由 ControllerName:ActionName:Method 变更为 RouteTemplate:Method，
        //      升级版本号可使所有旧格式缓存自动失效，无需手动清空 Redis。
        private const string CacheVersion = "v2";

        private static string CacheKey(long userId, PlatformType platformType) =>
            $"{CoreRedisConstants.UserContext.Format(userId, (int)platformType)}:{CacheVersion}";

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

            foreach (var p in Enum.GetValues<PlatformType>())
                await redisService.DeleteAsync(CacheKey(userId, p));
        }

        private async Task<UserContextCacheDto> BuildFromDbAsync(long userId, PlatformType platformType, CancellationToken cancellationToken)
        {
            var user = await dbContext.Set<User>()
                .Where(u => u.Id == userId)
                .Select(u => new { u.UserName, u.Email, u.IsEnable })
                .FirstOrDefaultAsync(cancellationToken);

            var roleIds = (await userRoleService.GetUserRoles(userId, platformType))
                .Select(ur => ur.RoleId)
                .Distinct()
                .ToList();

            var regionIds = await dbContext.Set<UserDepartment>()
                .Where(ud => ud.UserId == userId)
                .Select(ud => ud.DepartmentId)
                .ToListAsync(cancellationToken);

            // 预计算当前平台下该用户拥有的 API 权限集合，鉴权时直接读缓存，无需每次查 DB
            var apiPermissionKeys = new HashSet<string>();
            if (roleIds.Count > 0)
            {
                var menuIds = await dbContext.Set<Permission>()
                    .Where(p => roleIds.Contains(p.RoleId))
                    .Select(p => p.MenuId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                if (menuIds.Count > 0)
                {
                    var keys = await (
                        from mr in dbContext.Set<MenuResource>()
                        join ar in dbContext.Set<ApiResource>() on mr.ApiResourceId equals ar.Id
                        where menuIds.Contains(mr.MenuId)
                              && ar.RoutePattern != null
                              && ar.RequestMethod != null
                        select ar.RoutePattern.ToLower() + ":" + ar.RequestMethod.ToUpper()
                    ).Distinct().ToListAsync(cancellationToken);

                    apiPermissionKeys = new HashSet<string>(keys, StringComparer.OrdinalIgnoreCase);
                }
            }

            return new UserContextCacheDto
            {
                UserName = user?.UserName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                IsEnable = user?.IsEnable ?? false,
                RoleIds = roleIds,
                RegionIds = regionIds,
                ApiPermissionKeys = apiPermissionKeys
            };
        }
    }
}
