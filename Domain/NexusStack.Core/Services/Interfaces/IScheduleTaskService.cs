using NexusStack.Core.Entities.Schedules;
using NexusStack.EFCore.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Interfaces
{
    public interface IScheduleTaskService : IServiceBase<ScheduleTask>
    {
        /// <summary>
        /// 初始化定时任务
        /// </summary>
        /// <returns></returns>
        Task InitializeAsync();
    }
}
