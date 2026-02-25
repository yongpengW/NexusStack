using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Roles
{
    public class RoleQueryDto : PagedQueryModelBase
    {
        /// <summary>
        /// 角色状态，True 启用 False 禁用
        /// </summary>
        public bool? IsEnable { get; set; }

        ///// <summary>
        ///// 所属平台
        ///// </summary>
        //public PlatformType platformType { get; set; }
    }
}
