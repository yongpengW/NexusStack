using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums.OpenAppConfigs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.OpenAppConfigs
{
    public class AppNotificationConfig : AuditedEntity
    {
        /// <summary>
        /// OpenApiConfig表主键Id
        /// </summary>
        public long AppId { get; set; }

        /// <summary>
        /// Webhook名称
        /// </summary>
        [MaxLength(256)]
        public required string Name { get; set; }

        /// <summary>
        /// 通知类型
        /// </summary>
        public ApiNoticeType Type { get; set; }

        [MaxLength(256)]
        public string? Phones { get; set; }

        /// <summary>
        /// 通知地址
        /// </summary>
        [MaxLength(256)]
        public string? NoticeUrl { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
