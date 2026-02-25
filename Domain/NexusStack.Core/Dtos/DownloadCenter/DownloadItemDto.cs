using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.DownloadCenter
{
    public class DownloadItemDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string key { get; set; }
        public ExportState State { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
