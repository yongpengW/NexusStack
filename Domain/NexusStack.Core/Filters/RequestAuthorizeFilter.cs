using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusStack.Core.Constants;
using NexusStack.Core.Dtos.Users;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Models;
using NexusStack.Infrastructure.Options;
using System.Linq;
using System.Threading.Tasks;

namespace NexusStack.Core.Filters
{
    /// <summary>
    /// 请求接口权限过滤器：AuthenticationHandler 负责身份认证（Token），本过滤器负责基于 RBAC 的权限校验。
    /// 权限数据全部来自 HttpContext.Items 中的 UserContextCacheDto（由认证 Handler 写入），不再查 DB。
    /// 支持配置化的权限校验模式（Strict/Relaxed/RootOnly/Disabled）。
    /// </summary>
    public class RequestAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly ILogger<RequestAuthorizeFilter> _logger;
        private readonly ApiAuthorizationOptions _authOptions;

        public RequestAuthorizeFilter(
            ILogger<RequestAuthorizeFilter> logger,
            IOptions<ApiAuthorizationOptions> authOptions)
        {
            _logger = logger;
            _authOptions = authOptions.Value;
        }

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
                    context.Result = new RequestJsonResult(new RequestResultModel(AuthorizationConstants.StatusCodes.Unauthorized, AuthorizationConstants.ErrorMessages.OpenApiAuthFailed, null));
                return Task.CompletedTask;
            }

            // 身份认证失败（Token 无效或未传）
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("未认证用户访问受保护接口: {Path} {Method}",
                    context.HttpContext.Request.Path, context.HttpContext.Request.Method);
                context.Result = new RequestJsonResult(new RequestResultModel(AuthorizationConstants.StatusCodes.Unauthorized, AuthorizationConstants.ErrorMessages.PleaseLogin, null));
                return Task.CompletedTask;
            }

            // 从认证阶段写入的缓存中读取用户上下文（IsEnable + ApiPermissionKeys），不查 DB
            // 无论任何权限模式，用户上下文都必须存在
            if (!context.HttpContext.Items.TryGetValue(CoreClaimTypes.UserContextItemsKey, out var ctxObj)
                || ctxObj is not UserContextCacheDto userContext)
            {
                context.Result = new RequestJsonResult(new RequestResultModel(AuthorizationConstants.StatusCodes.Unauthorized, AuthorizationConstants.ErrorMessages.UserContextMissing, null));
                return Task.CompletedTask;
            }

            // 用户已被禁用（缓存失效后重建时会同步最新状态）
            // 无论任何权限模式，禁用状态都必须拦截，防止影响业务
            if (!userContext.IsEnable)
            {
                var userId = context.HttpContext.User.FindFirst(CoreClaimTypes.UserId)?.Value ?? "unknown";
                _logger.LogWarning("已禁用用户 {UserId} 尝试访问: {Path} {Method}",
                    userId, context.HttpContext.Request.Path, context.HttpContext.Request.Method);
                context.Result = new RequestJsonResult(new RequestResultModel(AuthorizationConstants.StatusCodes.Forbidden, string.Format(AuthorizationConstants.ErrorMessages.UserDisabled, userContext.UserName), null));
                return Task.CompletedTask;
            }

            // [SkipApiPermissionCheck] 跳过 RBAC 权限校验（系统基础接口：登出、获取权限等或者用于特殊场景测试等需求）
            if (context.ActionDescriptor.EndpointMetadata.Any(a => a is SkipApiPermissionCheckAttribute))
                return Task.CompletedTask;

            // Disabled 模式：跳过 API 权限校验（仅用于开发/测试，身份认证和用户状态已在前面完成验证）
            if (_authOptions.ApiPermissionMode == ApiPermissionMode.Disabled)
            {
                _logger.LogWarning("⚠️ API 权限校验处于完全开放模式（Disabled），已跳过 API Key 校验");
                return Task.CompletedTask;
            }

            // Relaxed 模式：跳过 API 权限校验，仅验证身份认证和用户启用状态
            if (_authOptions.ApiPermissionMode == ApiPermissionMode.Relaxed)
            {
                return Task.CompletedTask;
            }

            // RootOnly 模式：超级管理员直接放行，普通用户执行权限校验
            if (_authOptions.ApiPermissionMode == ApiPermissionMode.RootOnly && userContext.IsRoot)
            {
                return Task.CompletedTask;
            }

            // Strict 模式下的超级管理员绕过（通过配置控制，默认关闭）
            if (_authOptions.ApiPermissionMode == ApiPermissionMode.Strict 
                && _authOptions.EnableRootBypass 
                && userContext.IsRoot)
            {
                return Task.CompletedTask;
            }

            // RBAC 权限校验：从预计算的 API 权限集合中查找，O(1) 时间复杂度
            // Key 格式：RouteTemplate.ToLower():HTTPMETHOD，与 ApiResource.RoutePattern 及缓存构建逻辑一致。
            // 使用路由模板而非 ActionName，可正确区分同控制器内同名重载 Action（如两个 PostAsync 对应不同路由）。
            var cad = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            if (cad == null)
                return Task.CompletedTask;

            var routeTemplate = cad.AttributeRouteInfo?.Template?.ToLowerInvariant() ?? string.Empty;
            
            // 验证路由模板有效性
            if (string.IsNullOrEmpty(routeTemplate))
            {
                _logger.LogWarning("接口 {Action} 缺少有效的路由模板", cad.ActionName);
                context.Result = new RequestJsonResult(new RequestResultModel(AuthorizationConstants.StatusCodes.Forbidden, AuthorizationConstants.ErrorMessages.InsufficientPermission, null));
                return Task.CompletedTask;
            }

            var apiKey = $"{routeTemplate}:{context.HttpContext.Request.Method.ToUpperInvariant()}";
            if (!userContext.ApiPermissionKeys.Contains(apiKey))
            {
                var userId = context.HttpContext.User.FindFirst(CoreClaimTypes.UserId)?.Value ?? "unknown";
                _logger.LogWarning("用户 {UserId}({UserName}) 无权限访问 {ApiKey} (模式: {Mode})",
                    userId, userContext.UserName, apiKey, _authOptions.ApiPermissionMode);
                context.Result = new RequestJsonResult(new RequestResultModel(AuthorizationConstants.StatusCodes.Forbidden, AuthorizationConstants.ErrorMessages.InsufficientPermission, null));
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
