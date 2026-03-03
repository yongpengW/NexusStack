namespace NexusStack.Core.Constants
{
    /// <summary>
    /// 认证和授权相关的常量
    /// </summary>
    public static class AuthorizationConstants
    {
        public static class ErrorMessages
        {
            public const string PleaseLogin = "请先登录";
            public const string OpenApiAuthFailed = "OpenAPI认证失败";
            public const string UserContextMissing = "用户上下文缺失，请重新登录";
            public const string UserDisabled = "该用户[{0}]已被禁用，请联系IT管理员处理";
            public const string InsufficientPermission = "暂无访问该接口的权限";
        }

        public static class StatusCodes
        {
            public const int Unauthorized = 401;
            public const int Forbidden = 403;
        }
    }
}
