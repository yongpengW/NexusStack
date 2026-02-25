using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Files
{
    public class ExportFileQueryDto : PagedQueryModelBase
    {
        public string? FileName { get; set; }
        public int? State { get; set; }
        public List<string>? DateRanges { get; set; }
    }
}
