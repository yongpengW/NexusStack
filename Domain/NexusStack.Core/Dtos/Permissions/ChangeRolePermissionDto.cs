using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Permissions
{
    public class ChangeRolePermissionDto
    {
        /// <summary>
        /// 角色编号
        /// </summary>
        public long RoleId { get; set; }

        public PlatformType? PlatformType { get; set; }

        public long[] Menus { get; set; } = [];
    }
}
