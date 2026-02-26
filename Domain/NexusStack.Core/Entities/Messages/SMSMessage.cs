using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.Messages
{
    public class SMSMessage : AuditedEntity
    {
        public MessageType MessageType { get; set; }

        [MaxLength(64)]
        public string BizId { get; set; } = string.Empty;

        public MessageStatus MessageStatus { get; set; }

        public long Recipient { get; set; }

        public string Body { get; set; } = string.Empty;
    }
}
