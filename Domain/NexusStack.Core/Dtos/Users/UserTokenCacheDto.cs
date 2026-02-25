using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Users
{
    public class UserTokenCacheDto : UserTokenDto
    {
        /// <summary>
        /// UserToken Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 用户所有角色代码
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// 当前角色区域 Id
        /// </summary>
        public long RegionId { get; set; }

        /// <summary>
        /// 当前角色
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// Token所属平台
        /// </summary>
        public PlatformType PlatformType { get; set; }
    }
}
