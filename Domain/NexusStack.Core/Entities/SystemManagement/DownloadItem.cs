using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.SystemManagement
{
    public class DownloadItem : AuditedEntity
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(16)]
        public string Extension { get; set; } = string.Empty;

        public long Size { get; set; }

        [Required]
        [MaxLength(256)]
        public string bucket { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string key { get; set; } = string.Empty;

        public FileStorageType StorageType { get; set; }

        public ExportState State { get; set; }
    }
}
