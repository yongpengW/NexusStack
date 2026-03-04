using NexusStack.Core.Dtos.Roles;
using NexusStack.Core.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Dtos.Users
{
    public class CreateUserDto
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [MaxLength(64, ErrorMessage = "账号不能超过 64 个字符")]
        public required string UserName { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [MaxLength(64, ErrorMessage = "姓名不能超过 64 个字符")]
        public string? RealName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [MaxLength(64, ErrorMessage = "昵称不能超过 64 个字符")]
        public string? NickName { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        [MaxLength(15, ErrorMessage = "手机号不能超过 15 个字符")]
        public required string Mobile { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [MaxLength(64, ErrorMessage = "邮箱不能超过 64 个字符")]
        public required string Email { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public Gender Gender { get; set; }

        public string? Remark { get; set; }

        /// <summary>
        /// 用户角色
        /// </summary>
        public List<CreateUserRoleDto> UserRoles { get; set; } = [];

        /// <summary>
        /// 所属组织单元 Id 集合（当前指向 Region.Id）
        /// </summary>
        public long[] DepartmentIds { get; set; } = [];


        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; } = true;
    }
}
