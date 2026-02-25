using AutoMapper;
using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using NexusStack.Core.Dtos.Messages;
using NexusStack.Core.Entities.Messages;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Enums.Messages;
using NexusStack.Infrastructure.FileStroage;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.EventAlerts
{
    public class InternalMessageService(MainContext dbContext,
        IMapper mapper,
        ICurrentUser currentUser,
        IUserService userService,
        IFileStorageFactory storageFactory
        ) : ServiceBase<InternalMessage>(dbContext, mapper), IInternalMessageService, IScopedDependency
    {
        public async Task<int> DeleteAsync(long id)
        {
            // 找到主数据
            var internalMessage = await GetAsync(e => e.Id == id) ?? throw new Exception("站内信不存在");

            // 删除所有关联的InternalMessageRecipient数据
            var recipientsToUpdate = await dbContext.Set<InternalMessageRecipient>()
            .Where(r => r.MessageId == id)
            .ToListAsync();
            foreach (var recipient in recipientsToUpdate)
            {
                dbContext.Set<InternalMessageRecipient>().Remove(recipient);
            }

            // 删除主数据
            await base.DeleteAsync(internalMessage, default);

            // 保存更改
            return await dbContext.SaveChangesAsync();
        }

        public async Task<long> PostAsync(CreateInternalMessageDto model)
        {
            // 创建主数据
            var internalMessage = new InternalMessage
            {
                MessageType = MessageType.InternalMessage,
                Title = model.Title,
                Body = model.Content,
                PlatformTypes = model.PlatformTypes,
                CreatedBy = currentUser.UserId
            };
            if (model.Attachments != null && model.Attachments.Length > 0)
            {
                // 上传附件
                var aliyunStorage = storageFactory.GetStorage(FileStorageType.Aliyun);
                internalMessage.Attachments = "";

                foreach (var attachment in model.Attachments)
                {
                    var result = await aliyunStorage.UploadAsync(attachment.OpenReadStream(), "InternalMessages", attachment.FileName);
                    if (result.Success)
                    {
                        if (!string.IsNullOrEmpty(internalMessage.Attachments))
                        {
                            internalMessage.Attachments += ",";
                        }
                        internalMessage.Attachments += result.Data;
                    }
                }
            }
            await InsertAsync(internalMessage);
            // 创建关联的InternalMessageRecipient数据
            foreach (var shopId in model.StoreIds)
            {
                var recipient = new InternalMessageRecipient
                {
                    MessageId = internalMessage.Id,
                    StoreId = shopId
                };
                dbContext.Set<InternalMessageRecipient>().Add(recipient);
            }
            // 保存更改
            await dbContext.SaveChangesAsync();
            // 返回主数据的Id
            return internalMessage.Id;
        }

        public async Task<long> ReadAsync(long id)
        {
            var recipientsToUpdate = dbContext.Set<InternalMessageRecipient>().FirstOrDefault(x => x.Id == id);
            if (recipientsToUpdate != null)
            {
                recipientsToUpdate.RecipientUserId = currentUser.UserId;
                dbContext.Set<InternalMessageRecipient>().Update(recipientsToUpdate);
            }
            // 保存更改
            return await dbContext.SaveChangesAsync();
        }
    }
}
