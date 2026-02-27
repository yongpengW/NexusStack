using NexusStack.EFCore.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.Schedules
{
    /// <summary>
    /// 定时任务
    /// </summary>
    public class ScheduleTask : EntityBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        [MaxLength(256)]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 代码，默认为 Schedule 的类名
        /// </summary>
        [MaxLength(256)]
        public string? Code { get; set; }

        /// <summary>
        /// 是否启用的状态
        /// </summary>   
        public bool IsEnable { get; set; } = true;

        /// <summary>
        /// Cron 表达式
        /// </summary>
        public string? Expression { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTimeOffset NextExecuteTime { get; set; }

        /// <summary>
        /// 最后一次执行时间
        /// </summary>
        public DateTimeOffset LastExecuteTime { get; set; }
    }
}
