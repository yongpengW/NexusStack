using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using NexusStack.Core.Dtos.Users;
using NexusStack.Infrastructure.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NexusStack.Core.Filters
{
    /// <summary>
    /// 请求接口权限过滤器：AuthenticationHandler 负责身份认证（Token），本过滤器负责基于 RBAC 的权限校验。
    /// 权限数据全部来自 HttpContext.Items 中的 UserContextCacheDto（由认证 Handler 写入），不再查 DB。
    /// </summary>
    public class RequestAuthorizeFilter : IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // [AllowAnonymous] 直接放行
            if (context.ActionDescriptor.EndpointMetadata.Any(a => a.GetType() == typeof(AllowAnonymousAttribute)))
                return Task.CompletedTask;

            // OpenAPI 专用认证方案：只验证身份，不做 RBAC 校验
            var authorizeAttributes = context.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().ToList();
            var hasOpenApiAuthScheme = authorizeAttributes.Any(attr =>
                attr.AuthenticationSchemes != null &&
                attr.AuthenticationSchemes.Split(',').Contains("OpenAPIAuthentication"));

            if (hasOpenApiAuthScheme)
            {
                if (context.HttpContext.User.Identity?.IsAuthenticated != true)
                    context.Result = new RequestJsonResult(new RequestResultModel(StatusCodes.Status401Unauthorized, "OpenAPI认证失败", null));
                return Task.CompletedTask;
            }

            // 身份认证失败（Token 无效或未传）
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                context.Result = new RequestJsonResult(new RequestResultModel(StatusCodes.Status401Unauthorized, "请先登录", null));
                return Task.CompletedTask;
            }

            // 从认证阶段写入的缓存中读取用户上下文（IsEnable + ApiPermissionKeys），不查 DB
            if (!context.HttpContext.Items.TryGetValue(CoreClaimTypes.UserContextItemsKey, out var ctxObj)
                || ctxObj is not UserContextCacheDto userContext)
            {
                context.Result = new RequestJsonResult(new RequestResultModel(StatusCodes.Status401Unauthorized, "用户上下文缺失，请重新登录", null));
                return Task.CompletedTask;
            }

            // 用户已被禁用（缓存失效后重建时会同步最新状态）
            if (!userContext.IsEnable)
            {
                context.Result = new RequestJsonResult(new RequestResultModel(StatusCodes.Status403Forbidden, $"该用户[{userContext.UserName}]已被禁用，请联系IT管理员处理", null));
                return Task.CompletedTask;
            }

            // RBAC 权限校验：从预计算的 API 权限集合中查找，O(1) 时间复杂度
            // Key 格式：RouteTemplate.ToLower():HTTPMETHOD，与 ApiResource.RoutePattern 及缓存构建逻辑一致。
            // 使用路由模板而非 ActionName，可正确区分同控制器内同名重载 Action（如两个 PostAsync 对应不同路由）。
            var cad = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            if (cad == null)
                return Task.CompletedTask;

            var routeTemplate = cad.AttributeRouteInfo?.Template?.ToLowerInvariant() ?? string.Empty;
            var apiKey = $"{routeTemplate}:{context.HttpContext.Request.Method.ToUpperInvariant()}";
            if (!userContext.ApiPermissionKeys.Contains(apiKey))
            {
                context.Result = new RequestJsonResult(new RequestResultModel(StatusCodes.Status403Forbidden, "暂无访问该接口的权限", null));
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
