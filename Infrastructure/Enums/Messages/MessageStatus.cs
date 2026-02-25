using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NexusStack.Infrastructure.Enums.Messages
{
    public enum MessageStatus
    {
        [Description("准备中")]
        Pending = 0,

        [Description("发送中")]
        Sending = 5,

        [Description("发送完成")]
        Done = 10,

        [Description("发送失败")]
        Failed = 15
    }
}
