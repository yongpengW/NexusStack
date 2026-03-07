using Microsoft.Extensions.DependencyInjection;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.SeedData
{
    /// <summary>
    /// 初始化定时任务到数据库
    /// </summary>
    /// <param name="scopeFactory"></param>
    public class ScheduleTaskSeedData(IServiceScopeFactory scopeFactory) : ISeedData, ITransientDependency
    {
        /// <summary>
        /// 尽量在初始化数据之后再执行
        /// </summary>
        public int Order => 10;

        public string ConfigPath { get; set; } = string.Empty;

        public async Task ApplyAsync(SeedDataTask model)
        {
            using var scope = scopeFactory.CreateScope();
            var scheduleTaskService = scope.ServiceProvider.GetRequiredService<IScheduleTaskService>();

            await scheduleTaskService.InitializeAsync();
        }
    }
}
