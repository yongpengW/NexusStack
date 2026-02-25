using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Roles
{
    public class CreateUserRoleDto
    {
        /// <summary>
        /// 角色 Id
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 区域 Id
        /// </summary>
        public long RegionId { get; set; }
    }
}
