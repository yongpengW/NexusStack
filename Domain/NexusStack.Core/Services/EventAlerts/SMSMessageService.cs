using AutoMapper;
using NexusStack.Core.Dtos.Messages;
using NexusStack.Core.Entities.Messages;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.EventAlerts
{
    public class SMSMessageService(MainContext dbContext, IMapper mapper) : ServiceBase<SMSMessage>(dbContext, mapper), ISMSMessageService, IScopedDependency
    {
        public async Task<SMSMessageDto> GetEditInfo(long id)
        {
            var entity = await GetAsync(a => a.Id == id) ?? throw new Exception("数据不存在");
            var model = Mapper.Map<SMSMessageDto>(entity);
            return model;
        }
        public async Task<long> PostAsync(CreateSMSMessageDto model)
        {
            var entity = Mapper.Map<SMSMessage>(model);

            await InsertAsync(entity);
            return entity.Id;
        }

        public async Task<int> PutAsync(CreateSMSMessageDto model)
        {
            var entity = await GetAsync(a => a.Id == model.Id) ?? throw new Exception("你要修改的数据不存在");

            entity = Mapper.Map(model, entity);

            return await UpdateAsync(entity);
        }

        public async Task<int> PutAsync(long Id, string BizId, MessageStatus MessageStatus)
        {
            var entity = await GetAsync(a => a.Id == Id) ?? throw new Exception("你要修改的数据不存在");

            entity.BizId = BizId;
            entity.MessageStatus = MessageStatus;

            return await UpdateAsync(entity);
        }
    }
}
