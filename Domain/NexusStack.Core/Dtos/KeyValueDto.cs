using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos
{
    public class KeyValueDto
    {
        public string Key { get; set; }
        public string? Value { get; set; }
    }
    public class DecimalKeyValueDto
    {
        public decimal Key { get; set; }
        public decimal Value { get; set; }
    }

    public class IntKeyValueDto
    {
        public int Key { get; set; }
        public decimal Value { get; set; }
    }
}
