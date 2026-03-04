using NexusStack.Infrastructure.Enums;

namespace NexusStack.Infrastructure.Options
{
    /// <summary>
    /// API 授权配置
    /// </summary>
    public class ApiAuthorizationOptions : IOptions
    {
        public string SectionName => "Authorization";

        /// <summary>
        /// API 访问控制模式（默认：严格模式）
        /// 注意：身份认证、用户上下文加载、用户启用状态校验在所有模式下均强制执行。
        /// 此配置仅影响 RBAC API Key 匹配校验的行为：
        /// - Strict(0):   执行完整的 RBAC API 权限校验（推荐生产环境使用）
        /// - Relaxed(1):  跳过 API 权限校验，已认证且启用的用户均可访问
        /// - RootOnly(2): 超级管理员跳过校验，普通用户执行完整 RBAC 校验
        /// - Disabled(99): 跳过所有 API 权限校验（仅用于开发/测试环境）
        /// </summary>
        public ApiPermissionMode ApiPermissionMode { get; set; } = ApiPermissionMode.Strict;

        /// <summary>
        /// 是否启用超级管理员绕过权限检查（仅在 Strict 模式下有效）
        /// 默认：false（建议生产环境关闭，除非有特殊运维需求）
        /// </summary>
        public bool EnableRootBypass { get; set; } = false;
    }
}
