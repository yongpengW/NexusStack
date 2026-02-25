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
        /// 切换当前用户角色
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="userRoleId">用户角色Id(不是roleId)</param>
        /// <returns></returns>
        [HttpGet("switchrole/{platformType}/{userRoleId}")]
        public async Task<bool> SwitchRoleAsync(PlatformType platformType, long userRoleId)
        {
            var userId = base.CurrentUser.UserId;

            var roles = await userRoleService.GetUserRoles(userId, platformType);

            var role = roles.FirstOrDefault(a => a.Id == userRoleId);

            if (role is null)
            {
                throw new BusinessException("你要切换的角色已不存在");
            }

            if (await permissionService.GetCountAsync(item => item.RoleId == role.RoleId) == 0)
            {
                throw new BusinessException("该角色下没有任何权限");
            }

            var tokenHash = StringExtensions.EncodeMD5(this.CurrentUser.Token);

            var token = await userTokenService.GetAsync(a => a.TokenHash == tokenHash);

            if (token is null)
            {
                throw new BusinessException("请先登录");
            }

            token.RoleId = role.RoleId;
            //token.RegionId = role.RegionId;

            await userRoleService.ChangeDefaultRoleAsync(userRoleId, userId, platformType);

            //修改userToken 角色信息
            await userTokenService.UpdateAsync(token);

            //token.User = await userService.GetAsync(a => a.Id == token.UserId, includes: a => a.Include(c => c.UserRoles).ThenInclude(c => c.Role));

            var cacheData = this.Mapper.Map<UserTokenCacheDto>(token);

            await redisService.SetAsync(CoreRedisConstants.UserToken.Format(token.TokenHash), cacheData, token.ExpirationDate - DateTime.Now);

            return true;
        }

        [HttpGet("authUserLogin")]
        public async Task<StatusCodeResult> AuthUserLogin()
        {
            await AuthUserLoginAsync(new List<int> { 0, 1, 2 });
            return Ok();
        }

        private async Task AuthUserLoginAsync(List<int> allowedPlatforms)
        {
            if (CurrentUser.IsAuthenticated)
            {
                //检查用户是否存在(包含POS管理页面中删除的用户)
                if (!await userService.CheckUserExists(CurrentUser.UserId))
                {
                    var newuUer = Mapper.Map<User>(new CreateUserDto()
                    {
                        UserName = CurrentUser.UserName,
                        Mobile = string.Empty,
                        Email = CurrentUser.Email,
                        NickName = CurrentUser.UserName,
                        RealName = CurrentUser.UserName,
                        Gender = Gender.Unknown
                    });
                    newuUer.Id = CurrentUser.UserId;
                    newuUer.Password = newuUer.PasswordSalt = Guid.NewGuid().ToString().Replace("-", string.Empty);
                    newuUer.Avatar = newuUer.SignatureUrl = newuUer.Remark = string.Empty;
                    newuUer.LastLoginTime = newuUer.CreatedAt = newuUer.UpdatedAt = DateTime.Now;
                    newuUer.IsEnable = true;
                    newuUer.IsDeleted = false;
                    newuUer.Remark = "由SSO创建，系统自动生成";
                    var createUser = await userService.InsertAsync(newuUer);
                    var newUserRole = new UserRole
                    {
                        UserId = createUser.Id,
                        RoleId = SystemRoleConstants.DEFAULTROLEID,
                        IsDefault = true
                    };
                    //检查系统默认角色是否已存在
                    var existRoles = await userRoleService.CheckUserRoleExists(createUser.Id, SystemRoleConstants.DEFAULTROLEID);
                    if (!existRoles) await userRoleService.InsertAsync(newUserRole);
                }

                var user = await userService.GetAsync(x => x.Id == CurrentUser.UserId) ?? throw new ForbiddenException($"用户不存在或已删除, 请联系IT管理员处理");

                if (string.IsNullOrEmpty(user?.DepartmentIds))
                {
                    throw new ForbiddenException($"该用户[{user?.UserName}]未分配区域和门店信息, 请联系IT管理员配置");
                }

                if (!user.IsEnable)
                {
                    throw new ForbiddenException($"该用户[{user?.UserName}]已被禁用, 请联系IT管理员处理");
                }

                var departmentIds = user?.DepartmentIds?.Split('.').Select(x => long.Parse(x)).ToList();
                if (departmentIds == null || !departmentIds.Any())
                {
                    throw new ForbiddenException($"该用户[{user?.UserName}]未分配区域和门店信息, 请联系IT管理员配置");
                }

                var userRoles = await userRoleService.GetUserRoles(CurrentUser.UserId);
                //剔除禁用的用户角色
                userRoles = userRoles.Where(x => x.Role.IsEnable).ToList();
                if (userRoles.Count == 0)
                {
                    throw new ForbiddenException($"该用户[{user?.UserName}]未分配任何有效角色, 请联系IT管理员配置");
                }

                var platforms = new List<int>();
                userRoles.ForEach(x =>
                {
                    platforms.AddRange(x.Role.Platforms.Split(',').Select(int.Parse));
                });
                //var allowedPlatforms = new List<int> { 0, 1, 2 }; //允许登录的平台类型
                //判断是否有权限登录当前平台
                if (!platforms.Any(x => allowedPlatforms != null && allowedPlatforms.Contains(x)))
                {
                    throw new ForbiddenException($"该用户[{user?.UserName}]没有权限登录当前平台");
                }

                var regions = await regionService.GetListAsync(x => departmentIds.Contains(x.Id));
                //var shops = await shopService.GetListAsync(x => departmentIds.Contains(x.Id));
                var regionIds = regions.Select(x => x.Id).ToList();
                var childRegionIds = new List<long>();

                foreach (var id in regionIds)
                {
                    var childRegions = await regionService.GetListAsync(x => x.IdSequences.Contains(id.ToString()));

                    childRegionIds.AddRange(childRegions.Select(x => x.Id));
                }

                regionIds.AddRange(childRegionIds);
                regionIds = regionIds.Distinct().ToList();

                //var regionShops = await shopService.GetListAsync(x => regionIds.Contains(x.RegionId));
                //shops.AddRange(regionShops);

                //以Hash格式向redis中保存当前用户角色信息
                await redisService.HSetAsync(CoreRedisConstants.CurrentUserCache.Format(CurrentUser.UserId), CoreClaimTypes.UserId, CurrentUser.UserId, CoreRedisConstants.DefaultExpireSeconds);
                await redisService.HSetAsync(CoreRedisConstants.CurrentUserCache.Format(CurrentUser.UserId), CoreClaimTypes.UserName, CurrentUser.UserName, CoreRedisConstants.DefaultExpireSeconds);
                await redisService.HSetAsync(CoreRedisConstants.CurrentUserCache.Format(CurrentUser.UserId), CoreClaimTypes.Email, CurrentUser.Email, CoreRedisConstants.DefaultExpireSeconds);
                await redisService.HSetAsync(CoreRedisConstants.CurrentUserCache.Format(CurrentUser.UserId), CoreClaimTypes.PlatForms, string.Join(',', platforms.Distinct()), CoreRedisConstants.DefaultExpireSeconds);
                await redisService.HSetAsync(CoreRedisConstants.CurrentUserCache.Format(CurrentUser.UserId), CoreClaimTypes.Roles, string.Join(',', userRoles.Select(x => x.Role.Id).Distinct()), CoreRedisConstants.DefaultExpireSeconds);
                await redisService.HSetAsync(CoreRedisConstants.CurrentUserCache.Format(CurrentUser.UserId), CoreClaimTypes.Regions, string.Join(',', regions.Select(x => x.Id).Distinct()), CoreRedisConstants.DefaultExpireSeconds);
                //await redisService.HSetAsync(CoreRedisConstants.CurrentUserCache.Format(CurrentUser.UserId), CoreClaimTypes.Shops, string.Join(',', shops.Select(x => x.Id).Distinct()), CoreRedisConstants.DefaultExpireSeconds);
                await redisService.HSetAsync(CoreRedisConstants.CurrentUserCache.Format(CurrentUser.UserId), CoreClaimTypes.IsEnable, user?.IsEnable == true ? "1" : "0", CoreRedisConstants.DefaultExpireSeconds);
            }
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
            if (departmentIds == null || !departmentIds.Any())
            {
                throw new ForbiddenException($"当前用户[{user?.UserName}]未分配区域和门店信息, 请联系IT管理员配置");
            }

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
            var departmentIdsValue = userInfo?.DepartmentIdsValue?.Split('.').Select(a => a).ToArray();
            userInfo.DepartmentIds = departmentIdsValue;

            userInfo.UserRoles = userRoles.Select(a => new UserRoleDto
            {
                Id = a.Id,
                Platforms = a.Role.Platforms,
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
