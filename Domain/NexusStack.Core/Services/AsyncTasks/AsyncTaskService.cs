using AutoMapper;
using NexusStack.Core.Entities.AsyncTasks;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Options;
using NexusStack.RabbitMQ;
using NexusStack.RabbitMQ.EventBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace NexusStack.Core.Services.AsyncTasks
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
    /// <summary>
    /// 异步任务接口实现
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="mapper"></param>
    /// <param name="publisher"></param>
    public class AsyncTaskService(MainContext dbContext, IMapper mapper,
        IEventPublisher publisher,
        IEventCodeManager eventCodeManager) : ServiceBase<AsyncTask>(dbContext, mapper), IAsyncTaskService, IScopedDependency
    {
        private async Task<AsyncTask> GenerateTaskAsync<TData>(TData data, string code) where TData : new()
        {
            var task = new AsyncTask
            {
                Code = code,
                State = AsyncTaskState.Pending,
                Data = JsonSerializer.Serialize(data, JsonOptions.Default),
                ErrorMessage = string.Empty,
                Result = string.Empty,
                Remark = string.Empty,
            };
            return await this.InsertAsync(task);
        }

        public async Task<AsyncTask> CreateTaskAsync(object data, string code)
        {
            var eventType = eventCodeManager.GetEventType(code) ?? throw new InvalidOperationException($"未找到{code}对应的事件类型。");

            var task = await GenerateTaskAsync(data, code);

            if (Activator.CreateInstance(eventType, task) is not EventBase eventInstance)
            {
                throw new InvalidOperationException($"无法创建类型 {eventType.Name} 的实例。");
            }

            publisher.Publish(eventInstance);
            return task;
        }

        public async Task<bool> RetryAsync(AsyncTask task)
        {
            var eventType = eventCodeManager.GetEventType(task.Code) ?? throw new InvalidOperationException($"未找到{task.Code}对应的事件类型。");

            if (Activator.CreateInstance(eventType, task) is not EventBase eventInstance)
            {
                throw new InvalidOperationException($"无法创建类型 {eventType.Name} 的实例。");
            }

            publisher.Publish(eventInstance);
            return true;
        }
    }
}
