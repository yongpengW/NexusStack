using LinqKit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NexusStack.Core;
using NexusStack.Core.Dtos.Roles;
using NexusStack.Core.Dtos.Users;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure.Constants;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.Utils;
using NexusStack.Redis;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using StringExtensions = NexusStack.Infrastructure.Utils.StringExtensions;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// Token 管理(本地测试)
    /// </summary>
    public class TokenController(IUserTokenService userTokenService,
        IUserRoleService userRoleService,
        IUserService userService,
        IRedisService redisService,
        IPermissionService permissionService,
        IMenuService menuService,
        IRegionService regionService,
        IClaimsTransformation claimsTransformation,
        IConfiguration configuration,
        IRoleService roleService
    ) : BaseController
    {
        /// <summary>
        /// 获取图片验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet("captcha"), AllowAnonymous]
        public Task<CaptchaDto> GetCaptchaAsync()
        {
            return userTokenService.GenerateCaptchaAsync();
        }

        /// <summary>
        /// 账号密码登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("password"), AllowAnonymous]
        public async Task<UserTokenDto> PostAsync(PasswordLoginDto model)
        {
            if (!await userTokenService.ValidateCaptchaAsync(model.Captcha, model.CaptchaKey))
            {
                throw new BusinessException("验证码错误");
            }
            return await userTokenService.LoginWithPasswordAsync(model.UserName, model.Password, model.PlatformType);
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
                userToken.ExpirationDate = DateTime.Now;
                userToken.LoginType = LoginType.logout;
                await userTokenService.UpdateAsync(userToken);
                // 删除 Redis 中的缓存
                await redisService.DeleteAsync(CoreRedisConstants.UserToken.Format(userToken.TokenHash));
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
        /// 获取当前用户拥有的菜单权限列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("permission")]
        public async Task<List<RolePermissionDto>> GetCurrentUserPermissionAsync(PlatformType platformType)
        {
            return await GetCurrentUserPermissionAsync(CurrentUser.UserId, CurrentUser.Roles, platformType);
        }

        private async Task<List<RolePermissionDto>> GetCurrentUserPermissionAsync(long userId, long[] roles, PlatformType platformType)
        {
            var menuFilter = PredicateBuilder.New<Menu>(true).And(a => a.PlatformType == platformType);
            var query = (from p in permissionService.GetQueryable()
                         join m in menuService.GetExpandable().Where(menuFilter) on p.MenuId equals m.Id
                         join ur in userRoleService.GetQueryable() on p.RoleId equals ur.RoleId
                         join r in userService.GetQueryable() on ur.UserId equals r.Id
                         where ur.UserId == userId
                         && roles.Contains(ur.RoleId)
                         && m.IsVisible
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

        /// <summary>
        /// 切换当前用户角色（已废弃：V1 RBAC 采用多角色权限并集，不再支持切换角色）
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="userRoleId">用户角色Id(不是roleId)</param>
        /// <returns></returns>
        [HttpGet("switchrole/{platformType}/{userRoleId}")]
        public Task<bool> SwitchRoleAsync(PlatformType platformType, long userRoleId)
        {
            throw new BusinessException("系统已不再支持切换角色，权限由用户全部有效角色的并集决定。");
        }

        [HttpGet("getLoginUserInfo")]
        public async Task<UserDto> GetLoginUserInfo()
        {
            if (!CurrentUser.IsAuthenticated)
            {
                throw new ForbiddenException($"当前用户未通过认证");
            }

            var userRoles = await userRoleService.GetUserRoles(CurrentUser.UserId);
            var user = await userService.GetAsync(x => x.Id == CurrentUser.UserId);

            if (user == null || string.IsNullOrEmpty(user?.DepartmentIds))
            {
                throw new ForbiddenException($"当前用户[{user?.UserName}]未分配区域和门店信息, 请联系IT管理员配置");
            }

            var departmentIds = user?.DepartmentIds?.Split('.').Select(x => long.Parse(x)).ToList();

            var userInfo = new UserDto()
            {
                Id = user.Id,
                UserName = user.UserName,
                Mobile = user.Mobile,
                Email = user.Email,
                NickName = user.NickName,
                RealName = user.RealName,
                Gender = user.Gender,
                IsEnable = user.IsEnable,
                Remark = user.Remark,
                DepartmentIdsValue = user.DepartmentIds
            };

            var regions = await regionService.GetListAsync(x => departmentIds.Contains(x.Id));
            //var shops = await shopService.GetListAsync(x => departmentIds.Contains(x.Id));

            var regionDepartments = regions.Select(i => new UserDepartmentDto
            {
                DepartmentId = i.Id,
                DepartmentName = i.Name
            }).ToList();

            //var shopDepartments = shops.Select(s => new UserDepartmentDto
            //{
            //    DepartmentId = s.Id,
            //    DepartmentName = s.ShopName
            //}).ToList();

            //var departments = regionDepartments.Concat(shopDepartments).ToList();

            //userInfo.Departments = departments;
            var departmentIdsValue = userInfo.DepartmentIdsValue?.Split('.').Select(a => a).ToArray();
            userInfo.DepartmentIds = departmentIdsValue;

            userInfo.UserRoles = userRoles.Select(a => new UserRoleDto
            {
                Id = a.Id,
                Platforms = a.Role.Platforms.ToString(),
                RoleId = a.RoleId,
                RoleName = a.Role.Name,
                //RegionId = a.RegionId,
                //RegionName = a.Region.Name,
                //ShopId = a.ShopId,
                //ShopName = a.Shop == null ? "" : a.Shop.ShopName
            }).OrderBy(a => a.Id).ToList();

            return userInfo;
        }
    }
}
