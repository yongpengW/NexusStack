using AutoMapper;
using NexusStack.Core.Entities.Users;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Users
{
    public interface IRoleService : IServiceBase<Role>
    {
    }
    public class RoleService(MainContext dbContext, IMapper mapper) : ServiceBase<Role>(dbContext, mapper), IRoleService, IScopedDependency
    {
        public override async Task<Role> InsertAsync(Role entity, CancellationToken cancellationToken = default)
        {
            if (entity.Code.IsNullOrEmpty())
            {
                throw new ErrorCodeException(-1, "角色代码不能为空");
            }

            if (await ExistsAsync(a => a.Code == entity.Code))
            {
                throw new ErrorCodeException(-1, "角色代码已存在");
            }

            return await base.InsertAsync(entity, cancellationToken);
        }

        public override async Task<int> UpdateAsync(Role entity, CancellationToken cancellationToken = default)
        {
            if (entity.IsSystem)
            {
                throw new ErrorCodeException(-1, "系统角色不允许修改");
            }
            if (entity.Code.IsNullOrEmpty())
            {
                throw new ErrorCodeException(-1, "角色代码不能为空");
            }

            if (await ExistsAsync(a => a.Code == entity.Code && a.Id != entity.Id))
            {
                throw new ErrorCodeException(-1, "角色代码已存在");
            }

            var result = await base.UpdateAsync(entity, cancellationToken);
            return result;
        }
    }
}
