using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums.OpenAppConfigs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.OpenAppConfigs
{
    public class AppWebhookConfig : AuditedEntity
    {
        public long AppId { get; set; }

        /// <summary>
        /// Webhook名称
        /// </summary>
        [MaxLength(256)]
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// 第三方自定义Method
        /// </summary>
        [MaxLength(256)]
        public string? Method { get; set; }

        /// <summary>
        /// Webhook地址
        /// </summary>
        [MaxLength(256)]
        public string? HookUrl { get; set; }

        /// <summary>
        /// Hook类型
        /// </summary>
        public WebHookType Type { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
