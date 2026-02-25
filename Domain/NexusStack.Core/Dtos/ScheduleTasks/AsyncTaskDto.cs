using NexusStack.Infrastructure.Dtos;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.ScheduleTasks
{
    public class AsyncTaskDto : DtoBase
    {
        /// <summary>
        /// 任务状态
        /// </summary>
        public AsyncTaskState State { get; set; }

        /// <summary>
        /// 任务标识，根据该值判断处理方式
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 任务数据
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 任务返回数据
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 业务扩展ID
        /// </summary>
        public string? BusinessId { get; set; }
    }
}
