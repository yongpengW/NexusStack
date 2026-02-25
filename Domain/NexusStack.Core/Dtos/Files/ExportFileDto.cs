using NexusStack.Infrastructure.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Files
{
    public class ExportFileDto : DtoBase
    {
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ExpireDate { get; set; }
        public int State { get; set; }
        public string? StateName { get; set; }
        public decimal? Percent { get; set; }
        public string? Url { get; set; }
    }
}
