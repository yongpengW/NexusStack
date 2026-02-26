using NexusStack.EFCore.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.Messages
{
    public class InternalMessageRecipient : AuditedEntity
    {
        [Required]
        public long MessageId { get; set; }

        public long? RecipientUserId { get; set; }

        public virtual InternalMessage? Message { get; set; }
    }
}
