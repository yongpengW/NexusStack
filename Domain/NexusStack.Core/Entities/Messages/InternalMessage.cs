using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Enums.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.Messages
{
    public class InternalMessage : AuditedEntity
    {
        public MessageType MessageType { get; set; }

        [Required]
        [MaxLength(128)]
        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string? Attachments { get; set; }

        public PlatformType Platforms { get; set; }
    }
}
