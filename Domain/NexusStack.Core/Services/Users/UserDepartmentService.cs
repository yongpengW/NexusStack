using AutoMapper;
using NexusStack.Core.Entities.Users;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Users
{
    public interface IUserDepartmentService : IServiceBase<UserDepartment>
    {

    }
    public class UserDepartmentService(MainContext dbContext, IMapper mapper) : ServiceBase<UserDepartment>(dbContext, mapper), IUserDepartmentService, IScopedDependency
    {

    }
}
