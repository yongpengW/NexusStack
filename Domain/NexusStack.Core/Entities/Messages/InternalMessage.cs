using NexusStack.EFCore.Entities;
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

        [MaxLength(128)]
        public string Title { get; set; }

        public string Body { get; set; }

        public string? Attachments { get; set; }

        /// <summary>
        ///  Admin web: 1
        ///  安卓设备: 2
        /// </summary>

        public string PlatformTypes { get; set; }
    }
}
