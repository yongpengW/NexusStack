using AutoMapper;
using NexusStack.Core.Dtos.NotifyEvent;
using NexusStack.Core.Entities.Messages;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.EventAlerts
{
    public class NotifyEventService(MainContext dbContext, IMapper mapper) : ServiceBase<NotifyEvent>(dbContext, mapper), INotifyEventService, IScopedDependency
    {
        public async Task<List<NotifyEventTreeDto>> GetTreeListAsync()
        {

            var events = await GetListAsync();

            List<NotifyEventTreeDto> getChildren(long? parentId)
            {
                var children = events.Where(a => a.ParentId == parentId).OrderBy(a => a.Order).ToList();
                return children.Select(a =>
                {
                    var dto = Mapper.Map<NotifyEventTreeDto>(a);
                    dto.Children = getChildren(a.Id);
                    if (dto.Children.Count == 0)
                    {
                        dto.IsLeaf = true;
                        dto.Children = null;
                    }
                    return dto;
                }).ToList();
            }

            return getChildren(null);
        }

        public async Task<int> PutAsync(long id, CreateNotifyEventDto model)
        {
            var entity = await GetAsync(a => a.Id == id) ?? throw new Exception("你要修改的数据不存在");
            entity.MessageTypes = string.Join('.', model.MessageTypes);
            entity.NotifyRoles = string.Join('.', model.NotifyRoles);
            entity.IsActive = model.IsActive;
            return await UpdateAsync(entity);
        }
    }
}
