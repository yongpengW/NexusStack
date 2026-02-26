using Microsoft.EntityFrameworkCore;
using NexusStack.Core.Dtos;
using NexusStack.Core.Dtos.Messages;
using NexusStack.Core.Dtos.NotifyEvent;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Enums.Messages;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.EventAlerts
{
    public class EventAlertService(CommonSMSService commonSMSService,
        SMTPEmailService sMTPEmailService,
        IInternalMessageService internalMessageService,
        NotifyEventService notifyEventService,
        IUserRoleService userRoleService,
        IRegionService regionService,
        IOperationLogService operationLogService) : IScopedDependency
    {
        public async Task SendAlert(long notifyEventId, string title, string message, List<long> storeIds)
        {
            var notifyEvent = await notifyEventService.GetByIdAsync(notifyEventId);
            if (notifyEvent == null) { throw new BusinessException("数据不存在"); }
            ;
            var roles = notifyEvent.NotifyRoles.toBigIntList();
            if (roles.Count == 0) { throw new BusinessException("角色列表为空"); }
            var notifyMessageTypes = notifyEvent.MessageTypes.toBigIntList();
            if (notifyMessageTypes.Count == 0) { throw new BusinessException("消息类型列表为空"); }

            var users = userRoleService.GetQueryable()
                .Include(x => x.User)
                .Where(x => roles.Contains(x.RoleId)).Select(x => x.User).ToList();
            var regions = await regionService.GetListAsync();
            //var shops = await shopService.GetListAsync();

            var shopIds = new List<long>();
            var usersList = new List<User>();
            foreach (var user in users)
            {
                if (user != null)
                {
                    //usersList.Add(user);
                    //var departmentIds = user?.DepartmentIds?.Split('.').Select(x => long.Parse(x)).ToList();
                    //if (departmentIds != null)
                    //{
                    //    var userRegions = regions.Where(x => departmentIds.Contains(x.Id)).ToList();
                    //    var regionIds = userRegions.Select(x => x.Id).ToList();
                    //    var userShops = shops.Where(x => departmentIds.Contains(x.Id)).ToList();
                    //    var regionShops = shops.Where(x => regionIds.Contains(x.RegionId)).ToList();
                    //    shopIds.AddRange(userShops.Select(x => x.Id).ToList());
                    //    shopIds.AddRange(regionShops.Select(x => x.Id).ToList());
                    //    shopIds = shopIds.Distinct().ToList();
                    //}
                }
            }

            // send sms
            foreach (var user in usersList)
            {
                if (notifyMessageTypes.Contains((long)EventAlertType.SMS))
                {
                    await commonSMSService.SendSmsAsync(user.Mobile, MessageType.SystemMessage, message);
                }
            }

            // send email
            foreach (var user in usersList)
            {
                if (notifyMessageTypes.Contains((long)EventAlertType.Email))
                {
                    await sMTPEmailService.SendEmailAsync(new EmailAlertDto()
                    {
                        body = message,
                        subject = title,
                        toAddress = new List<string>() { user.Email }
                    });
                }
            }

            // send internal message
            if (notifyMessageTypes.Contains((long)EventAlertType.InternalMessage) && storeIds.Count > 0)
            {
                var log = new SystemLogDto() { title = "发送站内提醒" };
                log.entity = new SystemLogContent()
                {
                    Success = true,
                    Message = "发送站内提醒成功",
                    Entity = JsonConvert.SerializeObject(new { message, title, shopIds })
                };
                try
                {
                    await internalMessageService.PostAsync(new CreateInternalMessageDto()
                    {
                        Content = message,
                        Title = title,
                        StoreIds = shopIds
                    });
                }
                catch (Exception ex)
                {
                    log.logType = LogType.Error;
                    log.entity.Message = ex.Message;
                    log.entity.Success = false;
                }
                finally
                {
                    await operationLogService.SystemLogAsync(log);
                }
            }
        }
    }
}
