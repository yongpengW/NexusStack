using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure.Enums;

namespace NexusStack.Core.Authentication
{
    /// <summary>
    /// Authentik JWT Bearer 认证事件：JWT 验证通过后，根据 OIDC 声明查找本地用户并填充 Claims。
    /// </summary>
    public class AuthentikJwtBearerEvents : JwtBearerEvents
    {
        public AuthentikJwtBearerEvents()
        {
            OnTokenValidated = HandleTokenValidatedAsync;
        }

        private static async Task HandleTokenValidatedAsync(TokenValidatedContext ctx)
        {
            var provisioning = ctx.HttpContext.RequestServices.GetRequiredService<IAuthentikUserProvisioningService>();
            var userContextCache = ctx.HttpContext.RequestServices.GetRequiredService<IUserContextCacheService>();

            var sub = ctx.Principal?.FindFirstValue("sub");
            var email = ctx.Principal?.FindFirstValue(ClaimTypes.Email) ?? ctx.Principal?.FindFirstValue("email");
            var preferredUsername = ctx.Principal?.FindFirstValue("preferred_username");

            if (string.IsNullOrEmpty(sub))
            {
                ctx.Fail("Authentik JWT 缺少 sub 声明");
                return;
            }

            User? user;
            try
            {
                user = await provisioning.GetOrCreateAsync(sub, email, preferredUsername);
            }
            catch (Infrastructure.Exceptions.BusinessException ex)
            {
                ctx.Fail(ex.Message);
                return;
            }

            if (user == null)
            {
                ctx.Fail("无法创建或获取 Authentik 用户");
                return;
            }

            var platform = PlatformType.Admin;
            var bearerToken = ctx.Request.Headers.Authorization.ToString();
            if (bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                bearerToken = bearerToken["Bearer ".Length..].Trim();

            // 与内置 Token 方案一致：创建 UserToken、写入 Redis、预热 UserContext
            var userTokenService = ctx.HttpContext.RequestServices.GetRequiredService<IUserTokenService>();
            var expClaim = ctx.Principal?.FindFirstValue("exp");
            var expirationDate = expClaim != null && long.TryParse(expClaim, out var expUnix)
                ? DateTimeOffset.FromUnixTimeSeconds(expUnix)
                : DateTimeOffset.UtcNow.AddHours(10);
            await userTokenService.EnsureAuthentikSessionAsync(user.Id, bearerToken, platform, expirationDate, ctx.HttpContext.RequestAborted);

            var userContext = await userContextCache.GetOrSetAsync(user.Id, platform, cancellationToken: ctx.HttpContext.RequestAborted);
            ctx.HttpContext.Items[CoreClaimTypes.UserContextItemsKey] = userContext;

            var identity = (ClaimsIdentity)ctx.Principal!.Identity!;
            identity.AddClaim(new Claim(CoreClaimTypes.UserId, user.Id.ToString()));
            identity.AddClaim(new Claim(CoreClaimTypes.Token, bearerToken));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            identity.AddClaim(new Claim(CoreClaimTypes.TokenId, "0"));
            identity.AddClaim(new Claim(CoreClaimTypes.PlatFormType, ((int)platform).ToString()));
            identity.AddClaim(new Claim(CoreClaimTypes.UserName, userContext.UserName ?? string.Empty));
            identity.AddClaim(new Claim(CoreClaimTypes.Email, userContext.Email ?? string.Empty));
            identity.AddClaim(new Claim(CoreClaimTypes.IsRoot, userContext.IsRoot.ToString()));
        }
    }
}
