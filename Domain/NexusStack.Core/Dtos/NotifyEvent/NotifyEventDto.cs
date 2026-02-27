using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Infrastructure.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Dtos.NotifyEvent
{
    public class NotifyEventDto : DtoBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        [MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        [MaxLength(64)]
        public string Code { get; set; }

        /// <summary>
        /// 父级菜单
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 菜单类型
        /// </summary>
        public MenuType Type { get; set; }


        /// <summary>
        /// 菜单类型
        /// </summary>
        public MenuType EventType { get; set; }



        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(1024)]
        public string Remark { get; set; }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsActive { get; set; }

        public List<long> MessageTypes { get; set; }
        public List<long> NotifyRoles { get; set; }

        /// <summary>
        /// 是否叶子节点
        /// </summary>
        public bool IsLeaf { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

    }
}
