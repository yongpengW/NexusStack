using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Users
{
    public class UserQueryDto : PagedQueryModelBase
    {
        /// <summary>
        /// 角色编号
        /// </summary>
        public long? RoleId { get; set; }

        /// <summary>
        /// 区域id
        /// </summary>
        public long? RegionId { get; set; }

        /// <summary>
        /// 区域编码
        /// </summary>
        //public string RegionCode { get; set; }

        /// <summary>
        /// 包含下级
        /// </summary>
        public bool IncludeChilds { get; set; } = true;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsEnable { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public UserType UserType { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string? Mobile { get; set; }

        /// <summary>
        /// 权限code
        /// </summary>
        //public string PermissionCode { get; set; }
    }
}
