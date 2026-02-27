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
    /// <summary>
    /// 开放API配置服务
    /// </summary>
    public interface IAppConfigService : IServiceBase<AppConfig>
    {

    }
    public class AppConfigService(MainContext dbContext, IMapper mapper) : ServiceBase<AppConfig>(dbContext, mapper), IAppConfigService, IScopedDependency
    {

    }
}
