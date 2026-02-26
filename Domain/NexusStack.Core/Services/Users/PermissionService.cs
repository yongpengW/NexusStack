using AutoMapper;
using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using NexusStack.Core.Dtos.Menus;
using NexusStack.Core.Dtos.Permissions;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.SystemManagement;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Core.Services.Interfaces;

namespace NexusStack.Core.Services.Users
{
    public class PermissionService(MainContext dbContext, IMapper mapper, IMenuService menuService, IServiceBase<ApiResource> apiResourceService, IServiceBase<MenuResource> menuResourceService, IRoleService roleService) : ServiceBase<Permission>(dbContext, mapper), IPermissionService, IScopedDependency
    {
        /// <summary>
        /// 修改角色权限
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task ChangeRolePermissionAsync(ChangeRolePermissionDto model)
        {
            var role = await roleService.GetAsync(a => a.Id == model.RoleId);
            if (role is null)
            {
                throw new BusinessException("要授权的角色不存在");
            }

            var oldMenuIds = await menuService.GetQueryable()
                .Where(a => a.PlatformType == model.PlatformType && a.IsVisible)
                .Select(x => x.Id).ToListAsync();

            // 删除原有数据
            await BatchDeleteAsync(a => a.RoleId == model.RoleId && oldMenuIds.Contains(a.MenuId));

            var menus = await menuService.GetListAsync(x => model.Menus.Contains(x.Id));
            //var parentIds = menus.Where(x=> x.ParentId > 0).Select(x => x.ParentId).Distinct().ToArray();
            var parentIds = new List<long>();

            foreach (var item in menus)
            {
                if (!model.Menus.Contains(item.ParentId) && item.ParentId != 0)
                {
                    parentIds.Add(item.ParentId);
                }
            }

            parentIds = parentIds.Distinct().ToList();

            var permissions = parentIds.Select(a => new Permission
            {
                MenuId = a,
                RoleId = model.RoleId
            }).ToList();

            permissions.AddRange(model.Menus.Select(a => new Permission
            {
                MenuId = a,
                RoleId = model.RoleId
            }));

            await InsertAsync(permissions.Distinct());
        }

        /// <summary>
        /// 获取对象菜单权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<List<PermissionDto>> GetRolePermissionAsync(long roleId, PlatformType? platformType)
        {
            var role = await roleService.GetByIdAsync(roleId);
            if (role is null)
            {
                throw new BusinessException("当前角色不存在");
            }

            var query = from m in menuService.GetQueryable().Where(a => a.IsVisible
                            && (platformType.HasValue ? a.PlatformType == platformType
                                                      : (role.Platforms == PlatformType.All || (role.Platforms & a.PlatformType) != 0)))
                        join p in GetQueryable().Where(a => a.RoleId == roleId) on m.Id equals p.MenuId into pt
                        from pm in pt.DefaultIfEmpty()
                        select new PermissionDto
                        {
                            MenuId = m.Id,
                            MenuName = m.Name,
                            MenuParentId = m.ParentId,
                            MenuType = m.Type,
                            MenuOrder = m.Order,
                            HasPermission = pm != null ? true : false,
                            RoleId = pm != null ? pm.RoleId : roleId,
                            Id = pm != null ? pm.Id : 0,
                            MenuUrl = m.Url
                        };

            var permissions = await query.ToListAsync();

            Func<long, bool, List<PermissionDto>> getChildren = null;

            getChildren = (parentId, operate) =>
            {
                if (operate)
                {
                    return permissions.Where(a => a.MenuParentId == parentId && a.MenuType == MenuType.Operation).OrderBy(a => a.MenuOrder).ToList();
                }

                return permissions.Where(a => a.MenuParentId == parentId && a.MenuType != MenuType.Operation).OrderBy(a => a.MenuOrder).Select(a =>
                {
                    a.Children = getChildren(a.MenuId, false);
                    a.Operations = getChildren(a.MenuId, true);

                    if (a.Children.Count == 0)
                    {
                        a.Children = null;
                    }

                    return a;
                }).ToList();
            };

            return getChildren(0, false);
        }

        /// <summary>
        /// 获取对象菜单权限
        /// </summary>
        /// <param name="roleIds"></param>
        /// <returns></returns>
        public async Task<List<PermissionDto>> GetRolePermissionAsync(List<long> roleIds, PlatformType? platformType)
        {
            var roles = await roleService.GetListAsync(x => roleIds.Contains(x.Id));
            if (roles.Count == 0)
            {
                throw new BusinessException("当前角色不存在");
            }

            var permissions = new List<PermissionDto>();

            foreach (var role in roles)
            {
                var query = from m in menuService.GetQueryable().Where(a => a.IsVisible
                                && (platformType.HasValue ? a.PlatformType == platformType
                                                          : (role.Platforms == PlatformType.All || (role.Platforms & a.PlatformType) != 0)))
                            join p in GetQueryable().Where(a => a.RoleId == role.Id) on m.Id equals p.MenuId into pt
                            from pm in pt.DefaultIfEmpty()
                            select new PermissionDto
                            {
                                MenuId = m.Id,
                                MenuName = m.Name,
                                MenuParentId = m.ParentId,
                                MenuType = m.Type,
                                MenuOrder = m.Order,
                                HasPermission = pm != null ? true : false,
                                RoleId = pm != null ? pm.RoleId : role.Id,
                                Id = pm != null ? pm.Id : 0,
                                MenuUrl = m.Url
                            };

                permissions.AddRange(await query.ToListAsync());
            }

            return permissions;
        }

        public async Task<List<MenuTreeDto>> GetUserMenuTreeListAsync(ICurrentUser currentUser, PlatformType platformType)
        {
            // 根据当前用户，先拿到所有角色，再按平台过滤菜单权限
            var roleIds = await dbContext.Set<UserRole>()
                .Where(ur => ur.UserId == currentUser.UserId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            if (!roleIds.Any())
            {
                return new List<MenuTreeDto>();
            }

            var permissions = await GetRolePermissionAsync(roleIds, platformType);

            var menuIds = permissions.Where(x => x.HasPermission).Select(x => x.MenuId).Distinct().ToHashSet();

            var spec = Specifications<Menu>.Create();

            if (platformType != PlatformType.All)
            {
                spec.Query.Where(a => a.PlatformType == platformType);
            }

            spec.Query.Where(a => a.IsVisible && menuIds.Contains(a.Id));

            var menus = await menuService.GetListAsync(spec);

            List<MenuTreeDto> getChildren(long parentId)
            {
                var children = menus.Where(a => a.ParentId == parentId).OrderBy(a => a.Order).ToList();
                return children.Select(a =>
                {
                    var dto = Mapper.Map<MenuTreeDto>(a);

                    dto.Children = getChildren(a.Id);

                    if (dto.Children.Count == 0)
                    {
                        dto.Children = null;
                    }

                    return dto;
                }).ToList();
            }

            return getChildren(0);
        }

        public async Task<bool> JudgeHasPermissionAsync(string code, string menuCode)
        {
            var menu = await menuService.GetAsync(a => a.Code == menuCode);

            var resource = await apiResourceService.GetAsync(a => a.Code == code);

            return await menuResourceService.ExistsAsync(a => a.MenuId == menu.Id && a.ApiResourceId == resource.Id);
        }

        /// <summary>
        /// 判断指定用户在指定平台下，是否拥有某个 API 的访问权限
        /// </summary>
        public async Task<bool> HasApiPermissionAsync(long userId, PlatformType platformType, string controllerName, string actionName, string httpMethod)
        {
            // 1. 拿到用户在该平台下的所有角色
            var roleIds = await dbContext.Set<UserRole>()
                .Join(roleService.GetQueryable(), ur => ur.RoleId, r => r.Id, (ur, r) => new { ur, r })
                .Where(x => x.ur.UserId == userId
                            && (platformType == PlatformType.All || (x.r.Platforms & platformType) != 0))
                .Select(x => x.ur.RoleId)
                .Distinct()
                .ToListAsync();

            if (!roleIds.Any())
            {
                return false;
            }

            // 2. 这些角色拥有的菜单
            var menuIds = await dbContext.Set<Permission>()
                .Where(p => roleIds.Contains(p.RoleId))
                .Select(p => p.MenuId)
                .Distinct()
                .ToListAsync();

            if (!menuIds.Any())
            {
                return false;
            }

            // 3. 菜单关联到的 API 中，是否存在匹配当前控制器/Action/Method 的资源
            var hasPermission = await dbContext.Set<MenuResource>()
                .Join(dbContext.Set<ApiResource>(), mr => mr.ApiResourceId, ar => ar.Id, (mr, ar) => new { mr, ar })
                .AnyAsync(x =>
                    menuIds.Contains(x.mr.MenuId)
                    && x.ar.ControllerName == controllerName
                    && x.ar.ActionName == actionName
                    && x.ar.RequestMethod == httpMethod);

            return hasPermission;
        }
    }
}
