using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Schedules
{
    /// <summary>
    /// 每月计划任务
    /// </summary>
    public class FirstDayOfMonthSchedule : CronScheduleService
    {
        private readonly IServiceScopeFactory _serviceFactory;

        public FirstDayOfMonthSchedule(IServiceScopeFactory serviceFactory) : base(serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        /// <summary>
        /// 每月的1号 06:00 执行一次任务
        /// </summary>
        protected override string Expression { get; set; } = "0 0 6 1 * ?";

        protected override bool Singleton => true;

        protected override async Task ProcessAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<FirstDayOfMonthSchedule>>();

            var lastMonth = DateTimeOffset.UtcNow.AddMonths(-1);

            // 在这里执行你的任务逻辑，例如：每月一日发送上个月的销售报表给财务部门

        }
    }
}
