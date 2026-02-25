using AlibabaCloud.OpenApiClient.Models;
using AlibabaCloud.SDK.Dysmsapi20170525.Models;
using AlibabaCloud.SDK.Dysmsapi20170525;
using NexusStack.Core.Dtos;
using NexusStack.Core.Dtos.Messages;
using NexusStack.Core.Services.SystemManagement;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Enums.Messages;
using NexusStack.Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.EventAlerts
{
    public class CommonSMSService(GlobalSettingService globalSettingService,
        ISMSMessageService messageService,
        IOperationLogService operationLogService) : IScopedDependency
    {
        //获取配置
        private async Task<SmsConfigurationDto> GetSmsConfigurationAsync()
        {
            var setting = await globalSettingService.GetSingleSettingForApiAsync("SMS");
            if (setting == null || string.IsNullOrEmpty(setting.ConfigurationJson))
            {
                throw new Exception("SMS配置信息缺失。");
            }

            var config = JsonConvert.DeserializeObject<SmsConfigurationDto>(setting.ConfigurationJson);
            return config;
        }

        public async Task<Result<SendSmsResponse>> ReSendAsync(long id)
        {
            var result = new Result<SendSmsResponse>();
            var message = await messageService.GetByIdAsync(id);
            if (message == null) { throw new Exception("信息不存在"); }
            var log = new SystemLogDto() { title = "重发短信提醒" };
            var entity = new SystemLogContent()
            {
                Success = true,
                Message = "重新发短信提醒成功",
                Entity = JsonConvert.SerializeObject(message)
            };
            try
            {
                var response = await Send(message.Recipient.ToString(), message.MessageType, message.Body);
                string bizId = response.Body.BizId;
                var sendStatus = MessageStatus.Pending;
                if (!string.Equals(response.Body.Code, "OK"))
                {
                    sendStatus = MessageStatus.Failed;
                    throw new Exception(response.Body.Message);
                }
                sendStatus = MessageStatus.Sending;
                await messageService.PutAsync(message.Id, bizId, sendStatus);
            }
            catch (Exception ex)
            {
                log.logType = LogType.Error;
                entity.Message = ex.Message;
                entity.Success = false;
            }
            finally
            {
                log.entity = entity;
                await operationLogService.SystemLogAsync(log);
            }
            return result;
        }

        /// <summary>
        /// 发送模板短信
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="messageType"></param>
        /// <param name="templateParam"></param>
        /// <returns></returns>
        public async Task<Result<SendSmsResponse>> SendSmsAsync(string phoneNumber,
            MessageType messageType, string templateParam)
        {
            var result = new Result<SendSmsResponse>();
            var log = new SystemLogDto() { title = "发送短信提醒" };
            var entity = new SystemLogContent()
            {
                Success = true,
                Message = "发送短信提醒成功",
                Entity = JsonConvert.SerializeObject(new { phoneNumber, templateParam })
            };

            result.Success = true;

            try
            {
                var response = await Send(phoneNumber, messageType, templateParam);
                if (response != null)
                {
                    await AddMessage(phoneNumber, messageType, templateParam, response);
                    result.Data = response;
                }
            }
            catch (Exception ex)
            {
                log.logType = LogType.Error;
                entity.Message = ex.Message;
                entity.Success = false;
            }
            finally
            {
                log.entity = entity;
                await operationLogService.SystemLogAsync(log);
            }
            return result;
        }

        public async Task SyncMessage()
        {
            var messages = await messageService.GetListAsync(x => x.MessageStatus == MessageStatus.Pending || x.MessageStatus == MessageStatus.Sending);
            var config = await GetSmsConfigurationAsync();
            var client = new Client(new Config
            {
                AccessKeyId = config.AppId,
                AccessKeySecret = config.AppSecret,
            });

            foreach (var message in messages)
            {
                try
                {
                    var request = new QuerySendDetailsRequest
                    {
                        BizId = message.BizId,
                        PhoneNumber = message.Recipient.ToString(),
                        SendDate = message.CreatedAt.ToString("yyyyMMdd"),
                        PageSize = 10,
                        CurrentPage = 1
                    };

                    SMSSendStatus sendStatus = SMSSendStatus.Pending;
                    try
                    {
                        var sendRes = await client.QuerySendDetailsAsync(request);

                        if (sendRes != null && sendRes.StatusCode == 200)
                        {
                            sendRes.Body.SmsSendDetailDTOs.SmsSendDetailDTO.ForEach(dto =>
                            {
                                if (dto.SendStatus == 3)
                                {
                                    sendStatus = SMSSendStatus.Success;
                                    return;
                                }
                                else if (dto.SendStatus == 2)
                                {
                                    sendStatus = SMSSendStatus.Failure;
                                    return;
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    if (sendStatus == SMSSendStatus.Success)
                    {
                        message.MessageStatus = MessageStatus.Done;
                        message.UpdatedAt = DateTime.Now;
                        await messageService.UpdateAsync(message);
                    }
                    else if (sendStatus == SMSSendStatus.Failure)
                    {
                        message.MessageStatus = MessageStatus.Failed;
                        message.UpdatedAt = DateTime.Now;
                        await messageService.UpdateAsync(message);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// 发送模板短信
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="messageType"></param>
        /// <param name="templateParam"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<SendSmsResponse> Send(string phoneNumber,
          MessageType messageType, string templateParam)
        {
            var result = new Result<SendSmsResponse>();

            var config = await GetSmsConfigurationAsync();
            var client = new Client(new Config
            {
                AccessKeyId = config.AppId,
                AccessKeySecret = config.AppSecret,
            });
            var request = new SendSmsRequest
            {
                PhoneNumbers = phoneNumber,
                SignName = config.SignName,
                TemplateCode = config.TemplateCode,
                TemplateParam = templateParam
            };
            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(templateParam))
            {
                throw new Exception("发送失败，短信内容或者手机号为空。");
            }
            return await client.SendSmsAsync(request);
        }

        /// <summary>
        /// 添加模板短信记录
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="messageType"></param>
        /// <param name="templateParam"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task AddMessage(string phoneNumber,
            MessageType messageType,
            string templateParam,
            SendSmsResponse response)
        {
            string bizId = response.Body.BizId;
            var sendStatus = MessageStatus.Pending;
            if (!string.Equals(response.Body.Code, "OK"))
            {
                sendStatus = MessageStatus.Failed;
                throw new Exception(response.Body.Message);
            }
            sendStatus = MessageStatus.Sending;
            if (long.TryParse(phoneNumber, out long recipient))
            {
                var createModel = new CreateSMSMessageDto()
                {
                    MessageType = messageType,
                    MessageStatus = sendStatus,
                    Recipient = recipient,
                    Body = templateParam,
                    BizId = bizId
                };
                await messageService.PostAsync(createModel);
            }
        }

        // 查询短信发送状态
        public async Task<QuerySendDetailsResponse> QuerySendDetailsAsync(string phoneNumber, string bizId)
        {
            var config = await GetSmsConfigurationAsync();

            var client = new Client(new Config
            {
                AccessKeyId = config.AppId,
                AccessKeySecret = config.AppSecret,
            });

            var request = new QuerySendDetailsRequest
            {
                PhoneNumber = phoneNumber,
                BizId = bizId,
                SendDate = DateTime.Now.ToString("yyyyMMdd"),
                PageSize = 10,
                CurrentPage = 1
            };

            try
            {
                var response = await client.QuerySendDetailsAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"查询短信发送状态失败: {ex.Message}");
            }
        }
    }
}
