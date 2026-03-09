using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusStack.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace NexusStack.Core.Authentication
{
    /// <summary>
    /// SignalR 连接专用 Token 认证处理器。
    /// 优先从 querystring 的 access_token 读取（浏览器 WebSocket 常见），回退读取 Authorization: Bearer。
    /// </summary>
    public class RequestAuthenticationSignalRTokenHandler(
        IOptionsMonitor<RequestAuthenticationSignalRTokenSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserTokenService userTokenService,
        IUserContextCacheService userContextCacheService
    ) : AuthenticationHandler<RequestAuthenticationSignalRTokenSchemeOptions>(options, logger, encoder)
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = GetTokenFromQueryOrHeader();
            if (string.IsNullOrWhiteSpace(token))
            {
                return AuthenticateResult.NoResult();
            }

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
                new(CoreClaimTypes.PlatFormType, ((int)userToken.PlatformType).ToString()),
                new(CoreClaimTypes.UserName, userContext.UserName ?? string.Empty),
                new(CoreClaimTypes.Email, userContext.Email ?? string.Empty),
                new(CoreClaimTypes.IsRoot, userContext.IsRoot.ToString()),
            };

            var claimsIdentity = new ClaimsIdentity(claims, nameof(RequestAuthenticationSignalRTokenHandler));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        private string GetTokenFromQueryOrHeader()
        {
            // 仅对指定 Hub 路径允许 querystring access_token，避免误用到普通 HTTP API
            if (Request.Path.StartsWithSegments("/hubs/notification", StringComparison.OrdinalIgnoreCase))
            {
                // SignalR（浏览器）常把 token 放在 querystring：?access_token=xxx
                var queryToken = Request.Query["access_token"].ToString();
                if (!string.IsNullOrWhiteSpace(queryToken))
                {
                    return queryToken.Trim();
                }
            }

            var authorizationHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return string.Empty;
            }

            authorizationHeader = authorizationHeader.Trim();

            // 提取 Bearer token（移除 "Bearer " 前缀）
            const string scheme = "Bearer ";
            if (!authorizationHeader.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return authorizationHeader.Substring(scheme.Length).Trim();
        }
    }
}

