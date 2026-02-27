using NexusStack.Infrastructure.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.ScheduleTasks
{
    public class ScheduleTaskDto : DtoBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 定时任务类名
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Cron表达式
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// 下一次执行时间
        /// </summary>
        public DateTimeOffset NextExecuteTime { get; set; }

        /// <summary>
        /// 最后一次执行时间
        /// </summary>
        public DateTimeOffset LastExecuteTime { get; set; }


    }
}
