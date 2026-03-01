using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusStack.Core;
using NexusStack.Core.Dtos.Roles;
using NexusStack.Core.Dtos.Users;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure.Constants;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.Utils;
using NexusStack.Redis;
using StringExtensions = NexusStack.Infrastructure.Utils.StringExtensions;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// Token 管理(本地测试)
    /// </summary>
    public class TokenController(
        IUserTokenService userTokenService,
        IRedisService redisService,
        IUserContextCacheService userContextCacheService,
        IPermissionService permissionService,
        IMenuService menuService
    ) : BaseController
    {
        /// <summary>
        /// 账号密码登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("password"), AllowAnonymous]
        public Task<UserTokenDto> PostAsync(PasswordLoginDto model)
        {
            return userTokenService.LoginWithPasswordAsync(model.UserName, model.Password, model.PlatformType);
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        [HttpPost("signout")]
        public async Task<StatusCodeResult> SignoutAsync()
        {
            if (!this.CurrentUser.IsAuthenticated)
            {
                throw new BusinessException("请先登录");
            }

            var tokenHash = StringExtensions.EncodeMD5(this.CurrentUser.Token);

            // 修改 UserToken 中的 ExpirationDate 为当前时间
            var userToken = await userTokenService.GetAsync(a => a.TokenHash == tokenHash && a.UserId == this.CurrentUser.UserId);
            if (userToken != null)
            {
                userToken.ExpirationDate = DateTimeOffset.UtcNow;
                userToken.LoginType = LoginStatus.logout;
                await userTokenService.UpdateAsync(userToken);
                // 删除 Redis 中的缓存
                await redisService.DeleteAsync(CoreRedisConstants.UserToken.Format(userToken.TokenHash));
                await userContextCacheService.InvalidateAsync(CurrentUser.UserId, (PlatformType)CurrentUser.PlatformType);
            }

            return Ok();
        }

        /// <summary>
        /// 使用 Refresh Token 获取新的 Token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("Refresh"), AllowAnonymous]
        public Task<UserTokenDto> RefreshAsync(RefreshTokenDto model)
        {
            return userTokenService.RefreshTokenAsync(model.UserId, model.RefreshToken);
        }

        /// <summary>
        /// 获取当前用户拥有的菜单权限列表（仅返回当前登录平台下的有效权限）
        /// </summary>
        /// <returns></returns>
        [HttpGet("permission")]
        public async Task<List<RolePermissionDto>> GetCurrentUserPermissionAsync(PlatformType platformType)
        {
            // 直接使用认证阶段按平台过滤并缓存的 RoleIds，确保只返回当前平台下的权限
            var platformRoleIds = CurrentUser.RoleIds.ToList();
            if (platformRoleIds.Count == 0)
                return new List<RolePermissionDto>();

            var menuFilter = PredicateBuilder.New<Menu>(true).And(a => a.PlatformType == platformType);
            var query = (from p in permissionService.GetQueryable()
                         join m in menuService.GetExpandable().Where(menuFilter) on p.MenuId equals m.Id
                         where platformRoleIds.Contains(p.RoleId) && m.IsVisible
                         select new RolePermissionDto
                         {
                             MenuId = m.Id,
                             RoleId = p.RoleId,
                             MenuName = m.Name,
                             MenuCode = m.Code,
                             ParentId = m.ParentId,
                             Order = m.Order,
                             MenuUrl = m.Url,
                             Type = m.Type,
                             IconType = m.IconType,
                             ActiveIcon = m.ActiveIcon,
                             Icon = m.Icon,
                             IsExternalLink = m.IsExternalLink
                         })
                        .Distinct();
            var list = await query.ToListAsync();

            List<RolePermissionDto> getChildren(long parentId)
            {
                var children = list.Where(a => a.ParentId == parentId).OrderBy(a => a.Order).ToList();
                return children.Select(a =>
                {
                    a.Children = getChildren(a.MenuId);
                    return a;
                }).ToList();
            }

            return getChildren(0);
        }
    }
}
