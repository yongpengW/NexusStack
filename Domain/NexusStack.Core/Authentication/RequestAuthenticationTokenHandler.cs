using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusStack.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace NexusStack.Core.Authentication
{
#pragma warning disable CS0618 // 类型或成员已过时 - AuthenticationHandler 基类仍需要 ISystemClock
    public class RequestAuthenticationTokenHandler(
        IOptionsMonitor<RequestAuthenticationTokenSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IUserTokenService userTokenService
    ) : AuthenticationHandler<RequestAuthenticationTokenSchemeOptions>(options, logger, encoder, clock)
#pragma warning restore CS0618
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(token))
            {
                token = token.Trim();

                // 验证 Token 是否有效，并获取用户信息
                var userToken = await userTokenService.ValidateTokenAsync(token);
                if (userToken == null)
                {
                    return AuthenticateResult.Fail("Invalid Token!");
                }

                var claims = new List<Claim>
                {
                    new(CoreClaimTypes.RegionId, userToken.RegionId.ToString()),
                    new(CoreClaimTypes.UserId, userToken.UserId.ToString()),
                    new(CoreClaimTypes.Token, token),
                    new(CoreClaimTypes.RoleId, userToken.RoleId.ToString()),
                    //new(CoreClaimTypes.PopulationId, userToken.PopulationId.ToString()),
                    new(ClaimTypes.NameIdentifier, userToken.UserId.ToString()),
                    new(CoreClaimTypes.TokenId, userToken.Id.ToString()),
                    new(CoreClaimTypes.PlatFormType, userToken.PlatformType.ToString()),
                };

                // 将当前用户的所有角色添加到 Claims 中
                userToken.Roles.ForEach(a =>
                {
                    claims.Add(new Claim(ClaimTypes.Role, a));
                });

                var claimsIdentity = new ClaimsIdentity(claims, nameof(RequestAuthenticationTokenHandler));

                var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            return AuthenticateResult.NoResult();
        }
    }
}
