using AutoMapper;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Schedules
{
    public class SeedDataTaskCoreService(MainContext dbContext, IMapper mapper) : ServiceBase<SeedDataTask>(dbContext, mapper), ISeedDataTaskCoreService, IScopedDependency
    {

    }
}
