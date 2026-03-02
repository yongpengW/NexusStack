using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Entities.Users;
using NexusStack.Infrastructure.Dtos;
using System.Collections.Generic;

namespace NexusStack.Core.Dtos.Permissions
{
    public class PermissionDto : DtoBase
    {
        /// <summary>
        /// 权限所属对象编号
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 菜单排序
        /// </summary>
        public int MenuOrder { get; set; }

        /// <summary>
        /// 菜单编号
        /// </summary>
        public long MenuId { get; set; }

        /// <summary>
        /// 权限数据范围
        /// 仅 Menu / Operation 类型节点有实际意义；Directory / Subsystem 固定为 All。
        /// </summary>
        public DataRange DataRange { get; set; } = DataRange.All;

        /// <summary>
        /// 菜单名称
        /// </summary>
        public string MenuName { get; set; } = string.Empty;

        /// <summary>
        /// 父级菜单 Id
        /// </summary>
        public long MenuParentId { get; set; }

        /// <summary>
        /// 菜单 Url
        /// </summary>
        public string MenuUrl { get; set; } = string.Empty;

        /// <summary>
        /// 菜单类型
        /// </summary>
        public MenuType MenuType { get; set; }

        /// <summary>
        /// 是否拥有权限
        /// </summary>
        public bool HasPermission { get; set; }

        /// <summary>
        /// 下级
        /// </summary>
        public virtual List<PermissionDto>? Children { get; set; }

        /// <summary>
        /// 操作
        /// </summary>
        public virtual List<PermissionDto>? Operations { get; set; }
    }
}
