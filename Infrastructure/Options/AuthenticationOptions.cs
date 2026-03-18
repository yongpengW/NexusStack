namespace NexusStack.Infrastructure.Options
{
    /// <summary>
    /// 认证方式配置：BuiltIn（内置 Token）或 Authentik（OIDC）
    /// </summary>
    public class AuthenticationOptions : IOptions
    {
        public string SectionName => "Authentication";

        /// <summary>
        /// 认证提供者：BuiltIn | Authentik
        /// </summary>
        public string Provider { get; set; } = "BuiltIn";

        /// <summary>
        /// Authentik 配置（仅当 Provider=Authentik 时生效）
        /// </summary>
        public AuthentikOptions Authentik { get; set; } = new();
    }

    /// <summary>
    /// Authentik OIDC 配置
    /// </summary>
    public class AuthentikOptions
    {
        /// <summary>
        /// Authentik 颁发者地址，如 https://auth.example.com/application/o/nexusstack/
        /// </summary>
        public string Authority { get; set; } = string.Empty;

        /// <summary>
        /// 受众（Audience），与 Authentik Provider 中配置的 Client ID 一致
        /// </summary>
        public string Audience { get; set; } = string.Empty;
    }
}
