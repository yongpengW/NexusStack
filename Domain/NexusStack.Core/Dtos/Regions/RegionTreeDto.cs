using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Regions
{
    public class RegionTreeDto : RegionDto
    {
        /// <summary>
        /// 下级区域
        /// </summary>
        public List<RegionTreeDto> Children { get; set; } = new List<RegionTreeDto>();
    }
}
