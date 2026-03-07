using NexusStack.Core.Dtos.DownloadCenter;
using NexusStack.Core.Entities.AsyncTasks;
using NexusStack.EFCore.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Interfaces
{
    /// <summary>
    /// 异步任务接口定义
    /// </summary>
    public interface IAsyncTaskService : IServiceBase<AsyncTask>
    {
        /// <summary>
        /// 创建MQ异步任务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<AsyncTask> CreateTaskAsync(object data, string code);

        /// <summary>
        /// 重试异步任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        Task<bool> RetryAsync(AsyncTask task);
    }
}
