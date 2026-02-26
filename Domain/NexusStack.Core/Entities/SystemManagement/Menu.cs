using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.SystemManagement
{
    /// <summary>
    /// 菜单
    /// </summary>
    public class Menu : AuditedEntity
    {
        /// <summary>
        /// 菜单名称
        /// </summary>
        [MaxLength(256)]
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// 菜单标识
        /// </summary>
        [MaxLength(256)]
        [Required]
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
        /// 所属平台类型
        /// </summary>
        public PlatformType PlatformType { get; set; }

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
        /// 是否可见
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// 是否外链
        /// </summary>
        public bool IsExternalLink { get; set; }

        /// <summary>
        /// Id 序列
        /// </summary>
        public string IdSequences { get; set; } = string.Empty;

        /// <summary>
        /// 下级菜单
        /// </summary>
        public virtual List<Menu>? Children { get; set; }

        /// <summary>
        /// 父级菜单
        /// </summary>
        public virtual Menu? Parent { get; set; }

        /// <summary>
        /// 菜单接口
        /// </summary>
        public virtual IEnumerable<MenuResource>? Resources { get; set; }

        /// <summary>
        /// 所属系统Id
        /// </summary>
        public long SystemId { get; set; } = 0;
    }


    /// <summary>
    /// 菜单类型
    /// </summary>
    public enum MenuType
    {
        /// <summary>
        /// 子系统
        /// </summary>
        Subsystem = 1,
        /// <summary>
        /// 目录
        /// </summary>
        Directory = 2,
        /// <summary>
        /// 菜单
        /// </summary>
        Menu = 3,
        /// <summary>
        /// 操作 页面级功能操作
        /// 例：新增 删除 导入 导出等
        /// </summary>
        Operation = 4,
    }

    /// <summary>
    /// 菜单图标类型
    /// </summary>
    public enum MenuIconType
    {
        /// <summary>
        /// 图标
        /// </summary>
        Icon = 1,
        /// <summary>
        /// 图片
        /// </summary>
        Picture = 2
    }
}
