using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Schedules
{
    /// <summary>
    /// 每日计划任务
    /// </summary>
    public class DailySchedule : CronScheduleService
    {
        private readonly IServiceScopeFactory _serviceFactory;

        public DailySchedule(IServiceScopeFactory serviceFactory) : base(serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        /// <summary>
        /// 在每天的 0:05 执行一次任务
        /// </summary>
        protected override string Expression { get; set; } = "0 5 0 * * ?";

        protected override bool Singleton => true;

        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DailySchedule>>();
            var taskRecordService = scope.ServiceProvider.GetRequiredService<IServiceBase<ScheduleTaskRecord>>();
            var asyncTaskService = scope.ServiceProvider.GetRequiredService<IAsyncTaskService>();
            var operationLogService = scope.ServiceProvider.GetRequiredService<IOperationLogService>();

            // 清理日志和任务记录
            // 系统只保留最近两周的计划任务记录，三个月的操作日志，6个月的异步任务记录，其他的自动清理掉
            var delDate = DateTimeOffset.UtcNow.AddDays(-14);
            var deletedCount = await taskRecordService.BatchDeleteAsync(x => x.ExecuteEndTime <= delDate);

            var delDate2 = DateTimeOffset.UtcNow.AddMonths(-3);
            var deletedCount2 = await operationLogService.BatchDeleteAsync(x => x.CreatedAt <= delDate2);

            var delDate3 = DateTimeOffset.UtcNow.AddMonths(-6);
            var deletedCount3 = await asyncTaskService.BatchDeleteAsync(x => x.CreatedAt <= delDate3);

        }
    }
}
