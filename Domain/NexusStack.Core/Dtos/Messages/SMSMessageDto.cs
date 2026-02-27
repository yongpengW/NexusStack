using NexusStack.Infrastructure.Enums.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Messages
{
    public class SMSMessageDto
    {
        public long Id { get; set; }

        public MessageType MessageType { get; set; }

        public MessageStatus MessageStatus { get; set; }

        public string MessageTypeName { get; set; }

        public string MessageStatusName { get; set; }

        public long Recipient { get; set; }

        public string Body { get; set; }

        public DateTimeOffset SendTime { get; set; }
    }
}
