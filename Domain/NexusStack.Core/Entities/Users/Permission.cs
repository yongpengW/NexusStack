using NexusStack.Core.Entities.SystemManagement;
using NexusStack.EFCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Entities.Users
{
    /// <summary>
    /// 权限表RoleMenu(角色下勾选的所有菜单)
    /// </summary>
    public class Permission : AuditedEntity
    {
        /// <summary>
        /// 权限所属对象编号
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 菜单编号
        /// </summary>
        public long MenuId { get; set; }

        /// <summary>
        /// 权限数据范围
        /// </summary>
        public DataRange DataRange { get; set; }

        /// <summary>
        /// 菜单
        /// </summary>
        public virtual Menu? Menu { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public virtual Role? Role { get; set; }
    }

    /// <summary>
    /// 数据范围
    /// </summary>
    public enum DataRange
    {
        All = 0,
        CurrentAndSubLevels = 1,
        CurrentLevel = 2,
        CurrentAndParentLevels = 3,
        Self = 4,
    }
}
