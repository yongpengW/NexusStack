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
    public interface IAppWebhookConfigService : IServiceBase<AppWebhookConfig>
    {
    }
    public class AppWebhookConfigService(MainContext dbContext, IMapper mapper) : ServiceBase<AppWebhookConfig>(dbContext, mapper), IAppWebhookConfigService, IScopedDependency
    {
    }
}
