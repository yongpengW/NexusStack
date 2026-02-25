using NexusStack.Infrastructure.Enums.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Messages
{
    public class CreateSMSMessageDto
    {
        public long? Id { get; set; }

        public MessageType MessageType { get; set; }

        public MessageStatus MessageStatus { get; set; }

        public long Recipient { get; set; }

        public string BizId { get; set; }

        public string Body { get; set; }
    }
}
