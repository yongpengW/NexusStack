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
    public interface IApiNotificationConfigService : IServiceBase<ApiNotificationConfig>
    {
    }
    public class ApiNotificationConfigService(MainContext dbContext, IMapper mapper) : ServiceBase<ApiNotificationConfig>(dbContext, mapper), IApiNotificationConfigService, IScopedDependency
    {
    }
}
