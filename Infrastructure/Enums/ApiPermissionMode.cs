using System.ComponentModel;

namespace NexusStack.Infrastructure.Enums
{
    /// <summary>
    /// API 权限校验模式，仅控制 API 访问控制层（RBAC）的行为。
    /// 注意：身份认证、用户上下文加载、用户启用状态校验属于基础安全保障，
    ///       在所有模式下均强制执行，不受此配置影响。
    /// </summary>
    public enum ApiPermissionMode
    {
        /// <summary>
        /// 严格模式：执行完整的 RBAC API 权限校验（生产环境推荐）
        /// - ✓ 根据用户角色权限校验是否有权访问该 API
        /// - 可通过 EnableRootBypass 配置决定超级管理员是否绕过
        /// </summary>
        [Description("严格模式")]
        Strict = 0,

        /// <summary>
        /// 宽松模式：跳过 API 权限校验，所有已认证且启用的用户均可访问
        /// - ✗ 不校验 API 权限
        /// 适用场景：内部系统、对接口权限敏感度低的小团队
        /// </summary>
        [Description("宽松模式")]
        Relaxed = 1,

        /// <summary>
        /// 仅超管模式：超级管理员跳过 API 权限校验，普通用户执行完整 RBAC 校验
        /// - 超级管理员（IsRoot=true）✗ 不校验 API 权限
        /// - 普通用户 ✓ 执行完整的 RBAC API 权限校验
        /// 适用场景：需要为运维人员开放全部接口的场景
        /// </summary>
        [Description("仅超管模式")]
        RootOnly = 2,

        /// <summary>
        /// 完全开放模式：跳过所有 API 权限校验（仅用于开发/测试环境）
        /// - ✗ 不校验 API 权限
        /// ⚠️ 生产环境谨慎使用此模式
        /// </summary>
        [Description("完全开放模式")]
        Disabled = 99
    }
}
