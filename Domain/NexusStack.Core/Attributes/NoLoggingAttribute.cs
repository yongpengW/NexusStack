using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Attributes
{
    /// <summary>
    /// 不记录日志特性标识
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class NoLoggingAttribute : Attribute
    {

    }
}
