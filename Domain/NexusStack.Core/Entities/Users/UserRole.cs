using NexusStack.EFCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Entities.Users
{
    /// <summary>
    /// 用户角色关联信息
    /// </summary>
    public class UserRole : AuditedEntity
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 角色编号
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 是否默认角色
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// 角色信息
        /// </summary>
        public virtual Role? Role { get; set; }
    }
}
