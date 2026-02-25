using NexusStack.Core.Dtos.Regions;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.EFCore.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using File = NexusStack.Core.Entities.SystemManagement.File;

namespace NexusStack.Core.Services.Interfaces
{
    public interface IRegionService : IServiceBase<Region>
    {
        /// <summary>
        /// 获取区域树
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<List<RegionTreeDto>> GetTreeListAsync(RegionTreeQueryDto model);
    }
}
