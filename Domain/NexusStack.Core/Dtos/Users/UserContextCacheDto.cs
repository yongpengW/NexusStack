using System.Collections.Generic;

namespace NexusStack.Core.Dtos.Users
{
    /// <summary>
    /// 用户上下文缓存（按 UserId + PlatformType 缓存），用于鉴权成功后提供 Roles、Regions、API 权限集合等，不参与 Token 存储。
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
        /// 用户是否启用（缓存此字段，避免 Filter 每次查 DB）
        /// </summary>
        public bool IsEnable { get; set; } = true;

        /// <summary>
        /// 当前平台下该用户拥有的角色 Id 列表（RBAC 方案二：多角色并集）
        /// </summary>
        public List<long> RoleIds { get; set; } = new List<long>();

        /// <summary>
        /// 用户所属组织/地区 Id 列表（来自 UserDepartment.DepartmentId，当前指向 Region.Id）
        /// </summary>
        public List<long> RegionIds { get; set; } = new List<long>();

        /// <summary>
        /// 当前平台下该用户拥有的 API 权限集合，格式为 "routetemplate:HTTPMETHOD"（全小写路由+大写方法）。
        /// 与 ApiResource.RoutePattern + RequestMethod 及 RequestAuthorizeFilter 的 Key 构造逻辑一致，
        /// 使用路由模板而非 ActionName 可正确区分同控制器内同名重载 Action。
        /// 鉴权时直接从此集合查找，O(1) 时间复杂度，无需每次查 DB。
        /// </summary>
        public HashSet<string> ApiPermissionKeys { get; set; } = new HashSet<string>();
    }
}
