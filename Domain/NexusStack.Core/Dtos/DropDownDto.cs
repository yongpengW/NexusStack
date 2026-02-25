using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos
{
    public class IdDropDownDto
    {
        public long Id { get; set; }

        public string? Name { get; set; }
    }

    public class ValueDropDownDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}
