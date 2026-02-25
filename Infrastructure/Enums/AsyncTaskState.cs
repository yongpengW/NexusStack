using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NexusStack.Infrastructure.Enums
{
    /// <summary>
    /// 异步任务处理状态
    /// </summary>
    public enum AsyncTaskState
    {
        /// <summary>
        /// 待处理
        /// </summary>
        [Description("待处理")]
        Pending = 0,
        /// <summary>
        /// 处理中
        /// </summary>
        [Description("处理中")]
        InProgress = 1,
        /// <summary>
        /// 处理完成
        /// </summary>
        [Description("处理完成")]
        Completed = 2,
        /// <summary>
        /// 正在重试
        /// </summary>
        [Description("正在重试")]
        Retry = 3,
        /// <summary>
        /// 任务已创建
        /// </summary>
        [Description("已创建")]
        Created = 4,
        /// <summary>
        /// 处理失败
        /// </summary>
        [Description("处理失败")]
        Fail = 9
    }
}
