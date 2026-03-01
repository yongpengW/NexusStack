using System.Collections.Generic;

namespace NexusStack.Core.Dtos.Users
{
    /// <summary>
    /// 用户上下文缓存（按 UserId + PlatformType 缓存），用于鉴权成功后提供 Roles、Regions 等，不参与 Token 存储。
    /// </summary>
    public class UserContextCacheDto
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 当前平台下该用户拥有的角色 Id 列表（RBAC 方案二：多角色并集）
        /// </summary>
        public List<long> RoleIds { get; set; } = new List<long>();

        /// <summary>
        /// 用户所属组织/地区 Id 列表（来自 UserDepartment.DepartmentId，当前指向 Region.Id）
        /// </summary>
        public List<long> RegionIds { get; set; } = new List<long>();
    }
}
