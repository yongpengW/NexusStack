using Ardalis.Specification;
using Microsoft.AspNetCore.Mvc;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos.OpenApiConfigs;
using NexusStack.Core.Dtos.OpenAppConfigs;
using NexusStack.Core.Entities.OpenAppConfigs;
using NexusStack.Core.Services.OpenAppConfigs;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.Utils;
using NexusStack.Redis;
using System.Dynamic;
using X.PagedList;
using X.PagedList.Extensions;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// 开放应用配置中心
    /// </summary>
    public class OpenAppConfigController(ILogger<OpenAppConfigController> logger,
        IConfiguration configuration,
        IAppConfigService appConfigService,
        IAppNotificationConfigService appNotificationConfigService,
        IAppEventConfigService appEventConfigService,
        IAppWebhookConfigService appWebhookConfigService,
        IRedisService redisService) : BaseController
    {
        /// <summary>
        /// 获取配置json字符串
        /// </summary>
        /// <returns></returns>
        [HttpGet("config"), NoLogging]
        public dynamic GetConfigJson()
        {
            dynamic result = new ExpandoObject();

            var connString = configuration.GetValue<string>("Host");

            var test = configuration.GetValue<string>("Test");
            result.connString = connString;
            result.test = test;
            return result;
        }

        /// <summary>
        /// 获取开放应用列表
        /// </summary>
        [HttpGet("OpenApp/list"), NoLogging]
        public async Task<IPagedList<AppConfigViewDto>> GetOpenAppConfigListAsync([FromQuery] AppConfigQueryDto model)
        {
            var spec = Specifications<AppConfig>.Create();
            if (!string.IsNullOrEmpty(model.AppName))
            {
                spec.Query.Where(x => x.AppName.Contains(model.AppName));
            }

            var records = await appConfigService.GetListAsync<AppConfigViewDto>(spec);
            var views = new List<AppConfigViewDto>();

            foreach (var record in records)
            {
                views.Add(new AppConfigViewDto
                {
                    Id = record.Id,
                    AppName = record.AppName,
                    AppKey = record.AppKey,
                    Sessionkey = record.Sessionkey,
                    SecretKey = string.Empty,
                    AccessValidTime = record.AccessValidTime,
                    IsEnabled = record.IsEnabled,
                    Remark = record.Remark,
                    ShopId = record.ShopId
                });
            }

            var result = views.ToPagedList(model.Page, model.Limit);
            return result;
        }

        /// <summary>
        /// 获取开放app密钥
        /// </summary>
        [HttpGet("OpenApp/secretkey")]
        public async Task<string> GetOpenAppSecretKeyAsync(long id)
        {
            var config = await appConfigService.GetAsync(x => x.Id == id)
                ?? throw new BusinessException($"未找到Id为:[{id}]的配置");

            return config.SecretKey;
        }

        /// <summary>
        /// 添加开放app配置
        /// </summary>
        [HttpPost("OpenApp/add")]
        public async Task<StatusCodeResult> AddOpenAppConfigAsync(AppConfigCreateDto model)
        {
            var exists = await appConfigService.ExistsAsync(x => x.AppName == model.AppName);
            if (exists)
            {
                throw new BusinessException("AppName已存在");
            }

            var config = new AppConfig
            {
                AppName = model.AppName,
                AppKey = Guid.NewGuid().ToString("N"),
                Sessionkey = Guid.NewGuid().ToString("N"),
                SecretKey = EncryptionHelp.MD5Encrypt(Guid.NewGuid().ToString()),
                AccessValidTime = model.AccessValidTime,
                IsEnabled = model.IsEnabled,
                Remark = model.Remark
            };

            await appConfigService.InsertAsync(config);

            return Ok();
        }

        /// <summary>
        /// 编辑开放app配置
        /// </summary>
        [HttpPost("OpenApp/edit")]
        public async Task<long> UpdateOpenAppConfigAsync(AppConfigCreateDto model)
        {
            var config = await appConfigService.GetAsync(x => x.Id == model.Id)
                ?? throw new BusinessException($"未找到Id为:[{model.Id}]的配置");

            var exists = await appConfigService.GetAsync(x => x.AppName == model.AppName && x.Id != model.Id);
            if (exists != null)
            {
                throw new BusinessException("AppName已存在");
            }

            config.AppName = model.AppName;
            config.AccessValidTime = model.AccessValidTime;
            config.IsEnabled = model.IsEnabled;
            config.Remark = model.Remark;

            await appConfigService.UpdateAsync(config);

            return config.Id;
        }

        /// <summary>
        /// 重置app密钥
        /// </summary>
        [HttpPost("OpenApp/resetkey")]
        public async Task<StatusCodeResult> ResetAppSecretKeyAsync(long id)
        {
            var config = await appConfigService.GetAsync(x => x.Id == id)
                ?? throw new BusinessException($"未找到Id为:[{id}]的配置");

            config.SecretKey = EncryptionHelp.MD5Encrypt(Guid.NewGuid().ToString());

            await appConfigService.UpdateAsync(config);

            return Ok();
        }

        /// <summary>
        /// 获取开放app配置详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        [HttpGet("OpenApp/detail"), NoLogging]
        public async Task<OpenApiConfigDetailDto> GetOpenAppConfigDetailAsync(long id)
        {
            var config = await appConfigService.GetAsync(x => x.Id == id)
                ?? throw new BusinessException($"未找到AppId为:[{id}]的配置");

            var appNotificationConfig = await appNotificationConfigService.GetListAsync<AppNotificationConfigDto>(x => x.AppId == id);
            var appEventConfig = await appEventConfigService.GetListAsync<AppEventConfigDto>(x => x.AppId == id);
            var appWebhookConfig = await appWebhookConfigService.GetListAsync<AppWebhookConfigDto>(x => x.AppId == id);

            return new OpenApiConfigDetailDto
            {
                AppNotificationConfig = appNotificationConfig,
                AppEventConfig = appEventConfig,
                AppWebhookConfig = appWebhookConfig
            };
        }

        /// <summary>
        /// 编辑开放app通知配置（新增或修改）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        [HttpPost("OpenApp/Notification/edit")]
        public async Task<long> EditAppNotificationAsync(AppNotificationConfigDto model)
        {
            var config = await appNotificationConfigService.GetAsync(x => x.Id == model.AppId)
                ?? throw new BusinessException($"未找到AppId为:[{model.AppId}]的配置");

            var notificationConfig = await appNotificationConfigService.GetAsync(x => x.AppId == model.AppId && x.Id == model.Id);

            if (notificationConfig != null)
            {
                notificationConfig.Name = model.Name;
                notificationConfig.Type = model.Type;
                notificationConfig.Phones = model.Phones;
                notificationConfig.NoticeUrl = model.NoticeUrl;
                notificationConfig.IsEnabled = model.IsEnabled;
                await appNotificationConfigService.UpdateAsync(notificationConfig);

            }
            else
            {
                notificationConfig = new AppNotificationConfig
                {
                    AppId = model.AppId,
                    Name = model.Name,
                    Type = model.Type,
                    Phones = model.Phones,
                    NoticeUrl = model.NoticeUrl,
                    IsEnabled = model.IsEnabled
                };

                await appNotificationConfigService.InsertAsync(notificationConfig);
            }

            return notificationConfig.Id;
        }

        /// <summary>
        /// 删除应用通知配置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        [HttpPost("OpenApp/Notification/delete")]
        public async Task<bool> DeleteAppNotificationAsync(long id)
        {
            var config = await appNotificationConfigService.GetAsync(x => x.Id == id)
                ?? throw new BusinessException($"未找到Id为:[{id}]的配置");
            await appNotificationConfigService.DeleteAsync(config);
            return true;
        }

        /// <summary>
        /// Webhook 配置编辑（新增或修改）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        [HttpPost("OpenApp/Webhook/edit")]
        public async Task<long> EditWebhookConfigAsync(AppWebhookConfigDto model)
        {
            var config = await appWebhookConfigService.GetAsync(x => x.Id == model.AppId)
                ?? throw new BusinessException($"未找到AppId为:[{model.AppId}]的配置");

            var webhookConfig = await appWebhookConfigService.GetAsync(x => x.AppId == model.AppId && x.Id == model.Id);

            if (webhookConfig != null)
            {
                webhookConfig.Name = model.Name;
                webhookConfig.Method = model.Method;
                webhookConfig.HookUrl = model.HookUrl;
                webhookConfig.Type = model.Type;
                webhookConfig.IsEnabled = model.IsEnabled;
                await appWebhookConfigService.UpdateAsync(webhookConfig);

            }
            else
            {
                webhookConfig = new AppWebhookConfig
                {
                    AppId = model.AppId,
                    Name = model.Name,
                    Method = model.Method,
                    HookUrl = model.HookUrl,
                    Type = model.Type,
                    IsEnabled = model.IsEnabled
                };

                await appWebhookConfigService.InsertAsync(webhookConfig);
            }

            return webhookConfig.Id;
        }

        /// <summary>
        /// 删除Webhook配置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        [HttpPost("OpenApp/Webhook/delete")]
        public async Task<bool> DeleteWebhookConfigAsync(long id)
        {
            var config = await appWebhookConfigService.GetAsync(x => x.Id == id)
                ?? throw new BusinessException($"未找到Id为:[{id}]的配置");
            await appWebhookConfigService.DeleteAsync(config);
            return true;
        }

        /// <summary>
        /// App 事件配置编辑（新增或修改）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        [HttpPost("OpenApp/AppEvent/edit")]
        public async Task<long> EditAppEventConfigAsync(AppEventConfigDto model)
        {
            var config = await appEventConfigService.GetAsync(x => x.Id == model.AppId)
                ?? throw new BusinessException($"未找到AppId为:[{model.AppId}]的配置");

            var clearanceEventConfig = await appEventConfigService.GetAsync(x => x.AppId == model.AppId && x.Id == model.Id);

            if (clearanceEventConfig != null)
            {
                clearanceEventConfig.Name = model.Name;
                clearanceEventConfig.Method = model.Method;
                clearanceEventConfig.EventCode = model.EventCode;
                clearanceEventConfig.HookUrl = model.HookUrl;
                clearanceEventConfig.Type = model.Type;
                clearanceEventConfig.IsEnabled = model.IsEnabled;
                await appEventConfigService.UpdateAsync(clearanceEventConfig);

            }
            else
            {
                clearanceEventConfig = new AppEventConfig
                {
                    AppId = model.AppId,
                    Name = model.Name,
                    Method = model.Method,
                    EventCode = model.EventCode,
                    HookUrl = model.HookUrl,
                    Type = model.Type,
                    IsEnabled = model.IsEnabled
                };

                await appEventConfigService.InsertAsync(clearanceEventConfig);
            }

            return clearanceEventConfig.Id;
        }

        /// <summary>
        /// App 事件配置删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        [HttpPost("OpenApp/AppEvent/delete")]
        public async Task<bool> DeleteAppEventConfigAsync(long id)
        {
            var config = await appEventConfigService.GetAsync(x => x.Id == id)
                ?? throw new BusinessException($"未找到Id为:[{id}]的配置");
            await appEventConfigService.DeleteAsync(config);
            return true;
        }
    }
}
