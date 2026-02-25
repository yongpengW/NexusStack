using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.DownloadCenter
{
    public class ExportExcelRequestDto
    {
        public long? UserId { get; set; }
        public long? DownloadItemId { get; set; }
        public string QueryData { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    public class ExportExcelTypeMapDto
    {
        public string TypeName { get; set; } = string.Empty;
        public Type QueryModel { get; set; } = null!;
        public Type ExportModel { get; set; } = null!;
        public Func<object, Task<byte[]>> Method { get; set; } = null!;
        public bool Encrypt { get; set; } = false;
    }
}
