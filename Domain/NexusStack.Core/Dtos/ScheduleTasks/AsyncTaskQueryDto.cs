using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.ScheduleTasks
{
    public class AsyncTaskQueryDto : PagedQueryModelBase
    {
        /// <summary>
        /// 任务状态
        /// </summary>
        public AsyncTaskState? State { get; set; }

        /// <summary>
        /// 任务标识，根据该值判断处理方式
        /// </summary>
        public string? Code { get; set; }

        public string? Data { get; set; }
    }
}
