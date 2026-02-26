using NexusStack.EFCore.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.Users
{
    /// <summary>
    /// 用户
    /// </summary>
    public class User : AuditedEntity
    {
        /// <summary>
        /// 手机号码
        /// </summary>
        [MaxLength(15)]
        public string? Mobile { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [MaxLength(128)]
        public string? RealName { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 昵称
        /// </summary>
        [MaxLength(128)]
        public string? NickName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Password Salt
        /// </summary>
        [MaxLength(256)]
        public string PasswordSalt { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; } = true;

        /// <summary>
        /// 性别
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [MaxLength(512)]
        public string? Avatar { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [Required]
        [MaxLength(120)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime LastLoginTime { get; set; }

        /// <summary>
        /// 签名url
        /// </summary>
        [MaxLength(512)]
        public string? SignatureUrl { get; set; }

        /// <summary>
        /// 用户所属组织单元
        /// </summary>
        public virtual List<UserDepartment>? UserDepartments { get; set; }

        /// <summary>
        /// 用户所有角色
        /// </summary>
        public virtual List<Role>? Roles { get; set; }

        /// <summary>
        /// 角色用户关系数据
        /// </summary>
        public virtual List<UserRole>? UserRoles { get; set; }
    }

    /// <summary>
    /// 性别
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// 默认
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 男
        /// </summary>
        Male = 1,
        /// <summary>
        /// 女
        /// </summary>
        Female = 2
    }
}
