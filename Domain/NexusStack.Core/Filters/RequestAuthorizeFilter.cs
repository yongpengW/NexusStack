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
    /// 请求接口权限过滤器：AuthenticationHandler 负责身份认证（Token），本过滤器负责基于 RBAC 的权限校验
    /// </summary>
    public class RequestAuthorizeFilter(IPermissionService permissionService,
        ICurrentUser currentUser, IUserService userService) : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // 接口标记了[AllowAnonymous]，则不需要进行权限验证
            if (context.ActionDescriptor.EndpointMetadata.Any(a => a.GetType() == typeof(AllowAnonymousAttribute)))
            {
                return;
            }

            // 系统开放给第三方调用的接口（如 OpenAPI）可能会使用特定的认证方案（如 OpenAPIAuthentication），如果接口上标记了该认证方案，则只进行身份认证，不进行 RBAC 权限校验
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

            // 如果用户被禁用（从数据库实时检查）
            var user = await userService.GetAsync(a => a.Id == currentUser.UserId);
            if (user == null || !user.IsEnable)
            {
                context.Result = new RequestJsonResult(new RequestResultModel(StatusCodes.Status403Forbidden, $"该用户[{user?.UserName}]已被禁用, 请联系IT管理员处理", null));
                return;
            }

            // RBAC 权限校验：根据当前用户 + 平台 + 控制器/Action/HttpMethod 判断是否拥有访问权限
            var cad = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            if (cad == null)
            {
                // 非 MVC Action（如 Razor Page），默认放行或按需调整
                return;
            }

            var controllerName = cad.ControllerName;
            var actionName = cad.ActionName;
            var httpMethod = context.HttpContext.Request.Method;
            var platform = (Infrastructure.Enums.PlatformType)currentUser.PlatformType;

            var hasPermission = await permissionService.HasApiPermissionAsync(currentUser.UserId, platform, controllerName, actionName, httpMethod);
            if (!hasPermission)
            {
                context.Result = new RequestJsonResult(new RequestResultModel(StatusCodes.Status401Unauthorized, "暂无访问该接口的权限", null));
                return;
            }
        }
    }
}
