using NexusStack.EFCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Entities.Schedules
{
    public class ScheduleTaskRecord : EntityBase
    {
        /// <summary>
        /// 任务 Id
        /// </summary>
        public long ScheduleTaskId { get; set; }

        /// <summary>
        /// 是否成功，没报错即成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 如果执行失败，错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 执行开始时间
        /// </summary>
        public DateTime ExecuteStartTime { get; set; }

        /// <summary>
        /// 表达式计算时间
        /// </summary>
        public DateTime ExpressionTime { get; set; }

        /// <summary>
        /// 执行结束时间
        /// </summary>
        public DateTime ExecuteEndTime { get; set; }

        /// <summary>
        /// 定时任务
        /// </summary>
        public virtual ScheduleTask? ScheduleTask { get; set; }
    }
}
