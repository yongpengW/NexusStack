using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Dtos.Messages
{
    public class CreateInternalMessageDto
    {
        [MaxLength(128)]
        public string Title { get; set; }

        public string Content { get; set; }

        public IFormFile[]? Attachments { get; set; }

        public List<long> StoreIds { get; set; }

        public string PlatformTypes { get; set; }
    }
}
