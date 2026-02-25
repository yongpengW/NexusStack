using NexusStack.Infrastructure.Dtos;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Roles
{
    public class RoleDto : AuditedDtoBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 是否系统内置角色
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// 所属平台
        /// </summary>
        public PlatformType PlatformType { get; set; }

        public string Platforms { get; set; }

        public int[] PlatformArray
        {
            get
            {
                if (string.IsNullOrEmpty(Platforms))
                {
                    return Array.Empty<int>();
                }
                return Platforms.Split(',').Select(int.Parse).ToArray();
            }
        }
    }
}
