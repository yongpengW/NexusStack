using NexusStack.Infrastructure.Enums.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Messages
{
    public class InternalMessageDto
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public MessageType MessageType { get; set; }

        public string? Attachment { get; set; }

        public string[]? Attachments { get; set; }

        public string PlatformTypes { get; set; }

        public IEnumerable<InternalMessageRecipientDto?> Recipients { get; set; }

        public DateTimeOffset CreateAt { get; set; }
    }

    public class InternalMessageInboxDto
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public string PlatformTypes { get; set; }

        public MessageType MessageType { get; set; }
        public string? Attachment { get; set; }
        public string[]? Attachments { get; set; }
        public long? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Nickname { get; set; }

        public long StoreId { get; set; }
        public string StoreName { get; set; }

        public DateTimeOffset UpdateAt { get; set; }
        public DateTimeOffset CreateAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class InternalMessageRecipientDto
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string? UserName { get; set; }

        public long StoreId { get; set; }
        public string StoreName { get; set; }

        public DateTimeOffset UpdateAt { get; set; }
        public DateTimeOffset CreateAt { get; set; }

        public bool IsRead
        {
            get
            {
                return UserId != null;
            }
        }
    }
}
