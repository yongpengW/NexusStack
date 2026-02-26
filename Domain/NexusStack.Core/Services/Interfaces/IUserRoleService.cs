using NexusStack.Core.Entities.Users;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Interfaces
{
    /// <summary>
    /// 用户角色服务
    /// </summary>
    public interface IUserRoleService : IServiceBase<UserRole>
    {
        /// <summary>
        /// 获取用户下所有的角色
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="platformType"></param>
        /// <returns></returns>
        Task<List<UserRole>> GetUserRoles(long userId, PlatformType platformType);

        Task<List<UserRole>> GetUserRoles(long userId);

        /// <summary>
        /// 检查用户角色是否存在
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <param name="regionId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        Task<bool> CheckUserRoleExists(long userId, long roleId);
    }
}
