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
        //Task<AsyncTask> CreateTaskAsync<T>(object data, string code) where T : EventBase, new();

        Task<AsyncTask> CreateTaskAsync(object data, string code);

        Task<bool> RetryAsync(AsyncTask task);

        /// <summary>
        /// 发布清关推送异步任务
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task PushDeclarationEvent(long shopId, object data);

        /// <summary>
        /// 发布退货单推送异步任务
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task PushReturnEvent(long shopId, object data);

        /// <summary>
        ///  创建导出任务
        /// </summary>
        Task CreateExportExcelTaskAsync(ExportExcelRequestDto request);
    }
}
