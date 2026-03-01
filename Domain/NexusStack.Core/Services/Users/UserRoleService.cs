using Ardalis.Specification;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Users
{
    /// <summary>
    /// 用户角色
    /// </summary>
    public class UserRoleService(MainContext dbContext, IMapper mapper, IRoleService roleService) : ServiceBase<UserRole>(dbContext, mapper), IUserRoleService, IScopedDependency
    {
        public async Task<List<UserRole>> GetUserRoles(long userId, PlatformType platformType)
        {
            var query = from ur in GetQueryable()
                        join r in roleService.GetQueryable() on ur.RoleId equals r.Id
                        where ur.UserId == userId
                              && r.IsEnable
                              && (platformType == PlatformType.All || (r.Platforms & platformType) != 0)
                        select ur;

            var list = await query.Include(x => x.Role).ToListAsync();

            return list;
        }

        public async Task<List<UserRole>> GetUserRoles(long userId)
        {
            var query = from ur in GetQueryable()
                        where ur.UserId == userId
                        select ur;
            var list = await query.Include(x => x.Role).ToListAsync();
            return list;
        }

        public async Task<bool> CheckUserRoleExists(long userId, long roleId)
        {
            return await GetLongCountAsync(x => x.UserId == userId && x.RoleId == roleId) > 0;
        }
    }
}
