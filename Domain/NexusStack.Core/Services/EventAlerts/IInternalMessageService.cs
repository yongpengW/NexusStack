using NexusStack.Core.Dtos.Messages;
using NexusStack.Core.Entities.Messages;
using NexusStack.EFCore.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.EventAlerts
{
    public interface IInternalMessageService : IServiceBase<InternalMessage>
    {
        /// <summary>
        /// 新增站内信
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<long> PostAsync(CreateInternalMessageDto model);
        /// <summary>
        /// 已读站内信
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<long> ReadAsync(long id);

        /// <summary>
        /// 根据主键删除站内信
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ErrorCodeException"></exception>
        Task<int> DeleteAsync(long id);
    }
}
