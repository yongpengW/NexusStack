using AutoMapper;
using NexusStack.Core.Entities.OpenAppConfig;
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
    public interface IOpenApiConfigService : IServiceBase<OpenApiConfig>
    {

    }
    public class OpenApiConfigService(MainContext dbContext, IMapper mapper) : ServiceBase<OpenApiConfig>(dbContext, mapper), IOpenApiConfigService, IScopedDependency
    {

    }
}
