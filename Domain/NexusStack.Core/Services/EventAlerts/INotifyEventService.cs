using NexusStack.Core.Dtos.NotifyEvent;
using NexusStack.Core.Entities.Messages;
using NexusStack.EFCore.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.EventAlerts
{
    public interface INotifyEventService : IServiceBase<NotifyEvent>
    {
        /// <summary>
        /// 获取树
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<NotifyEventTreeDto>> GetTreeListAsync();

        Task<int> PutAsync(long id, CreateNotifyEventDto model);
    }
}
