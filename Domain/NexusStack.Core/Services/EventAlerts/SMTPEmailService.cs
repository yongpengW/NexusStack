using NexusStack.Core.Dtos;
using NexusStack.Core.Dtos.NotifyEvent;
using NexusStack.Core.Services.SystemManagement;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace NexusStack.Core.Services.EventAlerts
{
    public class SMTPEmailService(GlobalSettingService globalSettingService, IOperationLogService operationLogService) : IScopedDependency
    {
        //获取配置
        private async Task<SMTPOptions> GetEmailConfigurationAsync()
        {
            var setting = await globalSettingService.GetSingleSettingForApiAsync(GlobalSettingKey.Email.ToString());
            if (setting == null || string.IsNullOrEmpty(setting.ConfigurationJson))
            {
                throw new Exception("Email配置信息缺失。");
            }
            var config = JsonConvert.DeserializeObject<SMTPOptions>(setting.ConfigurationJson);
            return config;
        }

        public async Task<Result> SendEmailAsync(EmailAlertDto dto)
        {
            var result = new Result();
            var log = new SystemLogDto() { title = "发送邮件提醒" };
            var entity = new SystemLogContent()
            {
                Success = true,
                Message = "发送邮件提醒成功",
                Entity = JsonConvert.SerializeObject(dto)
            };
            try
            {
                if (dto.toAddress == null || dto.toAddress.Count == 0)
                {
                    throw new Exception("发送失败，收件人为空");
                }
                var setting = await GetEmailConfigurationAsync();
                var _smtpClient = new SmtpClient(setting.SMTPServerAddress, setting.SMTPServerPort)
                {
                    Credentials = new NetworkCredential(setting.senderEmail, setting.SMTPPassword),
                    EnableSsl = setting.needSSL
                };
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(setting.senderEmail),
                    Subject = dto.subject,
                    Body = dto.body,
                    IsBodyHtml = true
                };
                foreach (var email in dto.toAddress)
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        mailMessage.To.Add(email);
                    }
                }

                if (dto.Attachments != null)
                {
                    foreach (var attachment in dto.Attachments)
                    {
                        var stream = new MemoryStream(attachment.Content);
                        var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
                        mailMessage.Attachments.Add(mailAttachment);
                    }
                }

                await _smtpClient.SendMailAsync(mailMessage);
                result.Success = true;
            }
            catch (Exception ex)
            {
                log.logType = LogType.Error;
                entity.Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                entity.Success = false;
                result.Success = false;
                result.Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            }
            finally
            {
                log.entity = entity;
                await operationLogService.SystemLogAsync(log);
            }
            return result;
        }

        public class SMTPOptions
        {
            public string senderName { get; set; }
            public string senderEmail { get; set; }
            public string SMTPServerAddress { get; set; }
            public int SMTPServerPort { get; set; }
            public bool needVerifed { get; set; }
            public string SMTPUserName { get; set; }
            public string SMTPPassword { get; set; }
            public bool needSSL { get; set; }
        }
    }
}
