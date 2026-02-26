using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.Messages
{
    /// <summary>
    /// 通知事件
    /// </summary>
    public class NotifyEvent : AuditedEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        [MaxLength(256)]
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        [MaxLength(256)]
        [Required]
        public required string Code { get; set; }

        /// <summary>
        /// 父级
        /// </summary>
        public long? ParentId { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public NotifyEventType EventType { get; set; }
        /// <summary>
        /// 通知类型
        /// </summary>
        [MaxLength(1024)]
        public string? MessageTypes { get; set; }
        /// <summary>
        /// 通知角色
        /// </summary>
        [MaxLength(1024)]
        public string? NotifyRoles { get; set; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// 下级事件
        /// </summary>
        public virtual List<NotifyEvent>? Children { get; set; }

        /// <summary>
        /// 父级菜单
        /// </summary>
        public virtual NotifyEvent? Parent { get; set; }
    }
}
