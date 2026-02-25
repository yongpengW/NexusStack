using AutoMapper;
using NexusStack.Core.Entities.Messages;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.EventAlerts
{
    public class InternalMessageRecipientService(MainContext dbContext, IMapper mapper) : ServiceBase<InternalMessageRecipient>(dbContext, mapper), IInternalMessageRecipientService, IScopedDependency
    {
    }
}
