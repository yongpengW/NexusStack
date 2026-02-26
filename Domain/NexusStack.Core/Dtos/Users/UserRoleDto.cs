using NexusStack.Infrastructure.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Users
{
    public class UserRoleDto : DtoBase
    {
        /// <summary>
        /// 角色 Id
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// 所属业务平台（Flags 枚举，可多选）
        /// </summary>
        public PlatformType Platforms { get; set; }
    }
}
