using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NexusStack.Infrastructure.Enums
{
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogType
    {
        [Description("请求")]
        Request = 0,

        [Description("信息")]
        Info = 5,

        [Description("警告")]
        Warning = 10,

        [Description("错误")]
        Error = 15,

        [Description("严重错误")]
        SeriousError = 20
    }
}
