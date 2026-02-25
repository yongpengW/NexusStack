using NexusStack.Core.Dtos.Menus;
using NexusStack.Core.Dtos.Permissions;
using NexusStack.Core.Entities.Users;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Interfaces
{
    public interface IPermissionService : IServiceBase<Permission>
    {
        /// <summary>
        /// 获取角色下拥有的菜单权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        Task<List<PermissionDto>> GetRolePermissionAsync(long roleId, PlatformType? platformType);

        /// <summary>
        /// 获取多个角色下拥有的菜单权限
        /// </summary>
        /// <param name="roleIds"></param>
        /// <returns></returns>
        Task<List<PermissionDto>> GetRolePermissionAsync(List<long> roleIds, PlatformType? platformType);

        /// <summary>
        /// 获取菜单树（权限筛选）
        /// </summary>
        Task<List<MenuTreeDto>> GetUserMenuTreeListAsync(ICurrentUser currentUser, PlatformType platformType);

        /// <summary>
        /// 修改角色权限
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task ChangeRolePermissionAsync(ChangeRolePermissionDto model);

        /// <summary>
        /// 获取当前菜单是否拥有接口权限
        /// </summary>
        /// <param name="code">控制器action</param>
        /// <param name="menuCode">当前菜单或者操作的code</param>
        /// <returns></returns>
        Task<bool> JudgeHasPermissionAsync(string code, string menuCode);
    }
}
