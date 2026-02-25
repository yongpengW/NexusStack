using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Models
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; }
        public int TotalItemCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
