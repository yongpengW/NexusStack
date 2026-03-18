using Microsoft.EntityFrameworkCore;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.DbContexts;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.Utils;
using StringExtensions = NexusStack.Infrastructure.Utils.StringExtensions;

namespace NexusStack.Core.Services.Users
{
    /// <summary>
    /// Authentik 用户 JIT 供给：首次 Authentik 登录时检验用户信息，不存在则抛出
    /// </summary>
    public class AuthentikUserProvisioningService(IUserService userService) : IAuthentikUserProvisioningService, IScopedDependency
    {
        public async Task<User?> GetOrCreateAsync(string sub, string? email, string? preferredUsername)
        {
            var emailToUse = email?.Trim();
            if (string.IsNullOrEmpty(emailToUse))
                emailToUse = $"ext-{sub}@authentik.local";

            var userNameToUse = preferredUsername?.Trim();
            if (string.IsNullOrEmpty(userNameToUse))
                userNameToUse = emailToUse.Split('@')[0];

            // 先按用户名查找
            var user = await userService.GetAsync(u => u.UserName == userNameToUse);

            if (user != null)
                return user;

            // 按邮箱查找
            user = await userService.GetAsync(u => u.Email == emailToUse);

            if(user == null) throw new BusinessException($"未从系统中查询到用户[{userNameToUse}]信息，请联系IT管理员。");

            return user;
        }
    }
}
