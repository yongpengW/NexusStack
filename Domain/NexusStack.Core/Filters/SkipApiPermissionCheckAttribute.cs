using System;

namespace NexusStack.Core.Filters
{
    /// <summary>
    /// 跳过 API 权限检查（但仍需要身份认证）
    /// 用于系统基础接口：登出、获取权限、刷新token 等所有认证用户都需要的接口
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SkipApiPermissionCheckAttribute : Attribute
    {
    }
}
