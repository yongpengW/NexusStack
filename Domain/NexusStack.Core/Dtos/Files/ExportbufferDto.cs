using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Files
{
    public class ExportbufferDto
    {
        public string FileName { get; set; } = string.Empty;
        public string Buffer { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public long Size { get; set; } = 0;
    }
}
