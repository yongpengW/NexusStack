using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Entities.SystemManagement
{
    public class DownloadItem : AuditedEntity
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        public string bucket { get; set; }
        public string key { get; set; }
        public FileStorageType StorageType { get; set; }
        public ExportState State { get; set; }
    }
}
