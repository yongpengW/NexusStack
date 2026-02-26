using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Dtos.Roles
{
    public class CreateRoleDto
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "角色名称不能为空")]
        [MaxLength(64, ErrorMessage = "角色名称不能超过 64 个字符")]
        public string Name { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "角色代码不能为空")]
        [MaxLength(64, ErrorMessage = "角色代码不能超过 64 个字符")]
        public string Code { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; } = true;

        /// <summary>
        /// 是否是系统内置角色
        /// </summary>
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// 所属平台（Flags 枚举，可多选）
        /// </summary>
        public PlatformType Platforms { get; set; }
    }
}
