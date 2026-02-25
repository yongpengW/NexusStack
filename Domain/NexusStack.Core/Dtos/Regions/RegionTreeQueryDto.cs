using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Regions
{
    public class RegionTreeQueryDto : PagedQueryModelBase
    {
        /// <summary>
        /// 父级 Id
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 包含下级
        /// </summary>
        public bool IncludeChilds { get; set; }
    }
}
