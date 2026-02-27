using NexusStack.Infrastructure.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Files
{
    public class ExportFileDto : DtoBase
    {
        public string? Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? ExpireDate { get; set; }
        public int State { get; set; }
        public string? StateName { get; set; }
        public decimal? Percent { get; set; }
        public string? Url { get; set; }
    }
}
