using NexusStack.Infrastructure.Options;

namespace NexusStack.Core.Configuration
{
    /// <summary>
    /// Token 相关配置（集中管理所有时间配置）
    /// 使用 .NET Options Pattern，支持依赖注入和配置验证
    /// </summary>
    public class TokenOptions : IOptions
    {
        public string SectionName => "Token";

        /// <summary>
        /// Token 有效期（小时）
        /// </summary>
        public int ExpirationHours { get; set; } = 10;

        /// <summary>
        /// Refresh Token 有效期（月）
        /// </summary>
        public int RefreshTokenExpirationMonths { get; set; } = 1;

        /// <summary>
        /// 验证码有效期（分钟）
        /// </summary>
        public int CaptchaExpirationMinutes { get; set; } = 10;
    }
}
