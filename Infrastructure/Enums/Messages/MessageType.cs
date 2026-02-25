using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NexusStack.Infrastructure.Enums.Messages
{
    public enum MessageType
    {
        [Description("系统消息")]
        SystemMessage = 0,

        [Description("操作提醒")]
        OperationReminder = 5,

        [Description("站内消息")]
        InternalMessage = 10,
    }
}
