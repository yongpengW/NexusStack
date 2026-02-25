using NexusStack.Infrastructure.Enums.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Messages
{
    public class SendSMSMessageDto
    {
        public string PhoneNumber { get; set; }

        public MessageType MessageType { get; set; }

        public string TemplateParam { get; set; }
    }
}
