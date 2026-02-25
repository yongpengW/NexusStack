using NexusStack.Core.Dtos.Messages;
using NexusStack.Core.Entities.Messages;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Enums.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.EventAlerts
{
    public interface ISMSMessageService : IServiceBase<SMSMessage>
    {

        /// <summary>
        /// 获取编辑信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<SMSMessageDto> GetEditInfo(long id);
        /// <summary>
        /// 新增短信
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<long> PostAsync(CreateSMSMessageDto model);

        /// <summary>
        /// 修改短信
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<int> PutAsync(CreateSMSMessageDto model);
        Task<int> PutAsync(long id, string BizId, MessageStatus MessageStatus);
        /// <summary>
        /// 根据主键删除短信
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ErrorCodeException"></exception>
        Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default);
    }
}
