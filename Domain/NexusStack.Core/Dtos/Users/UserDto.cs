using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Entities.Users;
using NexusStack.Infrastructure.Dtos;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Users
{
    public class UserDto : AuditedDtoBase
    {
        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 是否设置密码
        /// </summary>
        public bool HasPassword { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime LastLoginTime { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        public string? Remark { get; set; }

        /// <summary>
        /// 用户所属组织单元（当前指向 Region）
        /// </summary>
        public List<UserDepartmentDto> Departments { get; set; } = new List<UserDepartmentDto>();
    }

    public class CurrentUserDto : UserDto
    {
        /// <summary>
        /// 当前登录用户的扩展信息（可按业务需要继续扩展）
        /// </summary>
        public string SignatureUrl { get; set; }
    }
}
