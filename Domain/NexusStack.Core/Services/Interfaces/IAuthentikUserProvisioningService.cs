using NexusStack.Core.Entities.Users;

namespace NexusStack.Core.Services.Interfaces
{
    /// <summary>
    /// Authentik 用户校验：根据 OIDC 声明查找本地用户，不存在则抛出 BusinessException。
    /// </summary>
    public interface IAuthentikUserProvisioningService
    {
        /// <summary>
        /// 根据 Authentik 声明查找本地用户。先按用户名、再按邮箱查找；查不到则抛出 BusinessException。
        /// </summary>
        /// <param name="sub">OIDC sub（唯一标识）</param>
        /// <param name="email">邮箱</param>
        /// <param name="preferredUsername">首选用户名</param>
        /// <returns>本地用户实体</returns>
        /// <exception cref="Infrastructure.Exceptions.BusinessException">用户不存在时抛出</exception>
        Task<User?> GetOrCreateAsync(string sub, string? email, string? preferredUsername);
    }
}
