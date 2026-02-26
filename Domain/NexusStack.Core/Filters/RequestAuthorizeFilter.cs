using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Core.Services.Users;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Filters
{
    /// <summary>
    /// 请求接口权限过滤器而AuthenticationHandler则是用户认证，token认证
    /// </summary>
    public class RequestAuthorizeFilter(IPermissionService permissionService,
        ICurrentUser currentUser, IUserService userService,
        IHttpContextAccessor httpContextAccessor) : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // 接口标记了[AllowAnonymous]，则不需要进行权限验证
            if (context.ActionDescriptor.EndpointMetadata.Any(a => a.GetType() == typeof(AllowAnonymousAttribute)))
            {
                return;
            }

            var authorizeAttributes = context.ActionDescriptor.EndpointMetadata
              .OfType<AuthorizeAttribute>()
              .ToList();

            var hasOpenApiAuthScheme = authorizeAttributes.Any(attr =>
               attr.AuthenticationSchemes != null &&
               attr.AuthenticationSchemes.Split(',').Contains("OpenAPIAuthentication"));

            if (hasOpenApiAuthScheme)
            {
                if (context.HttpContext.User.Identity?.IsAuthenticated != true)
                {
                    context.Result = new RequestJsonResult(new RequestResultModel(
                        StatusCodes.Status401Unauthorized,
                        "OpenAPI认证失败",
                        null));
                    return;
                }
                return;
            }

            // 未通过身份认证（Token 无效或未传）
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                context.Result = new RequestJsonResult(new RequestResultModel(StatusCodes.Status401Unauthorized, "请先登录", null));
                return;
            }

            // 如果用户被禁用
            if (currentUser == null || !currentUser.IsEnable)
            {
                context.Result = new RequestJsonResult(new RequestResultModel(StatusCodes.Status403Forbidden, $"该用户[{currentUser?.UserName}]已被禁用, 请联系IT管理员处理", null));
                return;
            }
        }
    }
}
