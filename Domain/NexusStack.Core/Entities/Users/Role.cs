using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.Users
{
    /// <summary>
    /// 角色
    /// </summary>
    public class Role : AuditedEntity
    {
        public Role()
        { }

        public Role(string name, string code)
        {
            Name = name;
            Code = code;
        }

        /// <summary>
        /// 名称
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 所属平台类型（Flags 枚举，支持多平台）
        /// </summary>
        public PlatformType Platforms { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 是否系统内置角色
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// 显示排序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; } = true;

        /// <summary>
        /// 角色所有用户
        /// </summary>
        public virtual List<User>? Users { get; set; }

        /// <summary>
        /// 角色用户关系数据
        /// </summary>
        public virtual List<UserRole>? UserRoles { get; set; }

        /// <summary>
        /// 角色权限
        /// </summary>
        public virtual List<Permission>? Permissions { get; set; }

        /// <summary>
        /// 所属系统Id
        /// </summary>
        public long SystemId { get; set; } = 0;
    }
}
