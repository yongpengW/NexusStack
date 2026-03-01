using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Core;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace NexusStack.Core.Authentication
{
public class RequestAuthenticationTokenHandler(
    IOptionsMonitor<RequestAuthenticationTokenSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IUserTokenService userTokenService,
    IUserContextCacheService userContextCacheService
) : AuthenticationHandler<RequestAuthenticationTokenSchemeOptions>(options, logger, encoder)
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(token))
            {
                token = token.Trim();

                // 验证 Token 是否有效，并获取用户信息（从 Redis / 数据库）
                var userToken = await userTokenService.ValidateTokenAsync(token);
                if (userToken == null)
                {
                    return AuthenticateResult.Fail("Invalid Token");
                }

                // 按 (UserId, PlatformType) 从 Redis 取用户上下文（Roles、Regions、UserName、Email），未命中则从 DB 构建并回写
                var userContext = await userContextCacheService.GetOrSetAsync(userToken.UserId, userToken.PlatformType);
                Context.Items[CoreClaimTypes.UserContextItemsKey] = userContext;

                var claims = new List<Claim>
                {
                    new(CoreClaimTypes.UserId, userToken.UserId.ToString()),
                    new(CoreClaimTypes.Token, token),
                    new(ClaimTypes.NameIdentifier, userToken.UserId.ToString()),
                    new(CoreClaimTypes.TokenId, userToken.Id.ToString()),
                    new(CoreClaimTypes.PlatFormType, userToken.PlatformType.ToString()),
                    new(CoreClaimTypes.UserName, userContext.UserName ?? string.Empty),
                    new(CoreClaimTypes.Email, userContext.Email ?? string.Empty),
                };

                var claimsIdentity = new ClaimsIdentity(claims, nameof(RequestAuthenticationTokenHandler));

                var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            return AuthenticateResult.NoResult();
        }
    }
}
