using Microsoft.AspNetCore.Mvc;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos.ScheduleTasks;
using NexusStack.Core.Entities.AsyncTasks;
using NexusStack.Core.Services.AsyncTasks;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.Utils;
using Ardalis.Specification;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// 异步任务
    /// </summary>
    public class AsyncTaskController(IAsyncTaskService asyncTaskService) : BaseController
    {
        /// <summary>
        /// 获取任务状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}"), NoLogging]
        public async Task<AsyncTaskDto> GetAsync(long id)
        {
            return await asyncTaskService.GetAsync<AsyncTaskDto>(a => a.Id == id);
        }

        /// <summary>
        /// 获取异步任务列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("list"), NoLogging]
        public async Task<IEnumerable<AsyncTaskDto>> GetAsyncTaskListAsync([FromQuery] AsyncTaskQueryDto model)
        {
            var spec = Specifications<AsyncTask>.Create();

            if (model.State.HasValue)
            {
                spec.Query.Where(x => x.State == model.State.Value);
            }
            if (!string.IsNullOrEmpty(model.Code))
            {
                spec.Query.Where(x => x.Code.Contains(model.Code));
            }
            if (!string.IsNullOrEmpty(model.Data))
            {
                spec.Query.Where(x => x.Data.Contains(model.Data));
            }

            spec.Query.OrderByDescending(x => x.CreatedAt);

            var asyncTaskList = await asyncTaskService.GetPagedListAsync<AsyncTaskDto>(spec, model.Page, model.Limit);


            return asyncTaskList;
        }

        /// <summary>
        /// 重试任务
        /// </summary>
        [HttpPost("retry")]
        public async Task<StatusCodeResult> RetryTaskAsync([FromBody] long id)
        {
            var task = await asyncTaskService.GetAsync(x => x.Id == id)
                ?? throw new BusinessException("任务不存在");

            if (task.State == AsyncTaskState.Completed)
            {
                throw new BusinessException("任务状态不正确");
            }

            task.State = AsyncTaskState.Retry;

            await asyncTaskService.UpdateAsync(task);
            await asyncTaskService.RetryAsync(task);

            task.ErrorMessage = null;
            task.Result = null;
            task.RetryCount++;

            await asyncTaskService.UpdateAsync(task);

            return Ok();
        }
    }
}
