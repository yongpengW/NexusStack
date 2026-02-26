using NexusStack.EFCore.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.SystemManagement
{
    public class GlobalSettings : AuditedEntity
    {
        /// <summary>
        /// AppId
        /// 用来区分系统，暂时用string，后期可能用雪花ID
        /// 例：WebApi系统appId为01， 某个门店的ScheduleappId为02，用以区分不同系统的设置
        /// </summary>

        public long AppId { get; set; } 

        /// <summary>
        /// 设置编码，用以区分设置类型便于打开不同的设置页面
        /// </summary>
        [MaxLength(128)]
        [Required]
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 系统设置的Metadata
        /// </summary>
        public string? ConfigurationJson { get; set; }
    }
}
