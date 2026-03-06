using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Infrastructure.Dtos;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Dtos.Menus
{
    public class MenuDto : AuditedDtoBase
    {
        /// <summary>
        /// 菜单名称
        /// </summary>
        [MaxLength(64)]
        public required string Name { get; set; }

        /// <summary>
        /// 菜单标识
        /// </summary>
        [MaxLength(64)]
        public required string Code { get; set; }

        /// <summary>
        /// 父级菜单
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 菜单类型
        /// </summary>
        public MenuType Type { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [MaxLength(1024)]
        public string? Icon { get; set; }

        /// <summary>
        /// 菜单图标类型
        /// </summary>
        public MenuIconType IconType { get; set; }

        /// <summary>
        /// 选中图标
        /// </summary>
        [MaxLength(1024)]
        public string? ActiveIcon { get; set; }

        /// <summary>
        /// 菜单选中图标类型
        /// </summary>
        public MenuIconType ActiveIconType { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        [MaxLength(1024)]
        public string? Url { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 是否可见 仅控制是否显示左侧菜单
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// 是否外链
        /// </summary>
        public bool IsExternalLink { get; set; }

        /// <summary>
        ///菜单所属平台类型
        /// </summary>
        public PlatformType PlatformType { get; set; }
    }
}
