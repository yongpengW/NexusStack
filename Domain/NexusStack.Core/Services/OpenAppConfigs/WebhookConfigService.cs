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
    public interface IWebhookConfigService : IServiceBase<AppWebhookConfig>
    {
    }
    public class WebhookConfigService(MainContext dbContext, IMapper mapper) : ServiceBase<AppWebhookConfig>(dbContext, mapper), IWebhookConfigService, IScopedDependency
    {
    }
}
