using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.AsyncTasks
{
    /// <summary>
    /// 异步任务
    /// </summary>
    public class AsyncTask : AuditedEntity
    {
        /// <summary>
        /// 任务状态
        /// </summary>
        public AsyncTaskState State { get; set; }

        /// <summary>
        /// 任务标识，根据该值判断处理方式
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 任务数据
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 任务返回数据
        /// </summary>
        public string? Result { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; }

    }
}
