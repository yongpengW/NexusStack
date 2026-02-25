using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos
{
    public class SelectOptionDto
    {
        public string label { get; set; }
        public long value { get; set; }
        public List<SelectOptionDto> children { get; set; } = new List<SelectOptionDto>();
    }
}
