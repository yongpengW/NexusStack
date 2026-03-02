using NexusStack.Core.Entities.Users;
using NexusStack.Infrastructure.Enums;

namespace NexusStack.Core.Dtos.Permissions
{
    /// <summary>
    /// 单条菜单权限提交项
    /// </summary>
    public class MenuPermissionItem
    {
        /// <summary>
        /// 菜单编号
        /// </summary>
        public long MenuId { get; set; }

        /// <summary>
        /// 权限数据范围
        /// </summary>
        public DataRange DataRange { get; set; } = DataRange.All;
    }

    public class ChangeRolePermissionDto
    {
        /// <summary>
        /// 角色编号
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 平台类型（可选；为空时按角色 Platforms 过滤）
        /// </summary>
        public PlatformType? PlatformType { get; set; }

        /// <summary>
        /// 角色在指定平台下勾选的菜单权限集合。
        /// 每个菜单必须携带 DataRange；半选父节点统一使用 DataRange.All。
        /// </summary>
        public MenuPermissionItem[] Menus { get; set; } = [];
    }
}
