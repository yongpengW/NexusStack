using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Regions
{
    public class RegionExportQueryDto
    {
        /// <summary>
        /// 行政区划 Id
        /// </summary>
        public long RegionId { get; set; }

        /// <summary>
        /// 选中项
        /// </summary>
        public long[] Ids { get; set; }
    }
}
