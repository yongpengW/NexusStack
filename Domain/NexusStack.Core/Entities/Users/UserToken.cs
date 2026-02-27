using NexusStack.Core.Entities.SystemManagement;
using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.Users
{
    /// <summary>
    /// 用户 Token
    /// </summary>
    public class UserToken : AuditedEntity
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Token Hash 用于查询
        /// </summary>
        [Required]
        [MaxLength(32)]
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>
        /// Refresh Token
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTimeOffset ExpirationDate { get; set; }

        /// <summary>
        /// 获取 Token IP 地址
        /// </summary>
        [MaxLength(32)]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 获取 Token UserAgent
        /// </summary>
        [MaxLength(1024)]
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// 当前获取 Token 平台
        /// </summary>
        public PlatformType PlatformType { get; set; }

        /// <summary>
        /// 登录方式
        /// </summary>
        public LoginMethodType LoginMethodType { get; set; }

        /// <summary>
        /// Refresh Token 是否有效
        /// </summary>
        public bool RefreshTokenIsAvailable { get; set; }

        /// <summary>
        /// Token 所属用户
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// 登录方式
        /// </summary>
        public LoginStatus LoginType { get; set; }
    }
}
