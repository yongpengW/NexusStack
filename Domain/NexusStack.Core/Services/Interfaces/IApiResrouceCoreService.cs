using NexusStack.Core.Dtos.Menus;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.EFCore.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Interfaces
{
    public interface IApiResrouceCoreService : IServiceBase<ApiResource>
    {
        /// <summary>
        /// 获取接口资源定义树列表
        /// </summary>
        /// <returns></returns>
        Task<List<MenuResourceDto>> GetTreeListAsync();
    }
}
