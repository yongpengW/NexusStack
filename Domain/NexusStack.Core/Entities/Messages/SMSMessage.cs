using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NexusStack.Core.Entities.Messages
{
    public class SMSMessage : AuditedEntity
    {
        public MessageType MessageType { get; set; }

        public string BizId { get; set; }

        public MessageStatus MessageStatus { get; set; }

        public long Recipient { get; set; }

        public string Body { get; set; }
    }
}
