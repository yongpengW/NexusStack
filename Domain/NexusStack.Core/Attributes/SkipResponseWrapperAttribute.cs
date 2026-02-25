using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Attributes
{
    /// <summary>
    /// 标记控制器或Action方法跳过统一返回结果包装
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SkipResponseWrapperAttribute : Attribute
    {
    }
}
