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

            // 其他需要登录验证的，则通过AuthenticationHandler进行用户认证
            if (context.HttpContext.User == null && context.HttpContext.User?.Identity?.IsAuthenticated == false)
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

            #region 废弃代码
            //else if (!(await userService.CheckCurrentExists(new CurrentUser(httpContextAccessor))))
            //{
            //    context.Result = new RequestJsonResult(new RequestResultModel(StatusCodes.Status401Unauthorized, "请重新登录", null));
            //    return;
            //}

            // 通过Menu-Code判断接口权限 暂未实现
            //if (context.ActionDescriptor is not null && context.ActionDescriptor is ControllerActionDescriptor descriptor)
            //{
            //    var namespaceStr = descriptor.ControllerTypeInfo.Namespace;
            //    var controllerName = descriptor.ControllerName;
            //    var actionName = descriptor.ActionName;

            //    var code = $"{namespaceStr}.{controllerName}.{actionName}";

            //    var menuCode = string.Empty;
            //    if (context.HttpContext.Request.Headers.ContainsKey("Menu-Code") && !string.IsNullOrEmpty(context.HttpContext.Request.Headers["Menu-Code"]))
            //    {
            //        menuCode = context.HttpContext.Request.Headers["Menu-Code"].ToString();
            //    }

            //    // 通过menuCode找到菜单Id，通过code找到接口Id 
            //    var hasPermission = false;

            //    //有些操作是不在菜单下面的，则默认有访问接口的权限 
            //    if (string.IsNullOrEmpty(menuCode))
            //    {
            //        hasPermission = true;
            //    }
            //    else
            //    {
            //        hasPermission = true; // await permissionService.JudgeHasPermissionAsync(code, menuCode);
            //    }

            //    if (hasPermission)
            //    {
            //        return;
            //    }

            //    context.Result = new RequestJsonResult(new RequestResultModel(StatusCodes.Status403Forbidden, "暂无权限", null));
            //    await Task.CompletedTask;
            //}
            #endregion
        }
    }
}
