using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NexusStack.Infrastructure.Enums.Messages
{
    /// <summary>
    /// SMS发送状态
    /// </summary>
    public enum SMSSendStatus
    {
        [Description("等待回执")]
        Pending = 1,

        [Description("发送失败")]
        Failure = 2,

        [Description("发送成功")]
        Success = 3,
    }
}
