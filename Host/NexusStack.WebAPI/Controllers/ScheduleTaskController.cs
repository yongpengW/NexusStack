using Microsoft.AspNetCore.Mvc;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos.ScheduleTasks;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure.Exceptions;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// 定时任务
    /// </summary>
    /// <param name="scheduleTaskService"></param>
    public class ScheduleTaskController(IScheduleTaskService scheduleTaskService) : BaseController
    {
        /// <summary>
        /// 定时任务列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, NoLogging]
        public async Task<List<ScheduleTaskDto>> GetListAsync()
        {
            return await scheduleTaskService.GetListAsync<ScheduleTaskDto>();
        }

        /// <summary>
        /// 启用定时任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("Enable/{id}")]
        public async Task<StatusCodeResult> ItemEnableAsync(long id)
        {
            var entity = await scheduleTaskService.GetByIdAsync(id);

            if (entity is null)
            {
                throw new BusinessException("您要启用的数据不存在");
            }

            entity.IsEnable = true;

            await scheduleTaskService.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// 禁用定时任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("Disable/{id}")]
        public async Task<StatusCodeResult> ItemDisableAsync(long id)
        {
            var entity = await scheduleTaskService.GetByIdAsync(id);

            if (entity is null)
            {
                throw new BusinessException("您要禁用的数据不存在");
            }

            entity.IsEnable = false;

            await scheduleTaskService.UpdateAsync(entity);

            return Ok();
        }
    }
}
