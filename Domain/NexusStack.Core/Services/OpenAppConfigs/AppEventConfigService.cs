using AutoMapper;
using NexusStack.Core.Entities.OpenAppConfigs;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.OpenAppConfigs
{
    public interface IAppEventConfigService : IServiceBase<AppEventConfig>
    {
    }
    public class AppEventConfigService(MainContext dbContext, IMapper mapper) : ServiceBase<AppEventConfig>(dbContext, mapper), IAppEventConfigService, IScopedDependency
    {
    }
}
