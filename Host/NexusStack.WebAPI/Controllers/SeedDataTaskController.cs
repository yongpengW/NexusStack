using Microsoft.AspNetCore.Mvc;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos.ScheduleTasks;
using NexusStack.Core.Entities.Schedules;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Exceptions;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// 种子数据初始化
    /// </summary>
    /// <param name="seedDataTaskService"></param>
    public class SeedDataTaskController(IServiceBase<SeedDataTask> seedDataTaskService) : BaseController
    {
        /// <summary>
        /// 种子数据任务列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, NoLogging]
        public async Task<List<SeedDataTaskDto>> GetListAsync()
        {
            return await seedDataTaskService.GetListAsync<SeedDataTaskDto>();
        }

        /// <summary>
        /// 启用种子数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("Enable/{id}")]
        public async Task<StatusCodeResult> ItemEnableAsync(long id)
        {
            var entity = await seedDataTaskService.GetByIdAsync(id);

            if (entity is null)
            {
                throw new BusinessException("您要启用的数据不存在");
            }

            entity.IsEnable = true;

            await seedDataTaskService.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// 禁用种子数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("Disable/{id}")]
        public async Task<StatusCodeResult> ItemDisableAsync(long id)
        {
            var entity = await seedDataTaskService.GetByIdAsync(id);

            if (entity is null)
            {
                throw new BusinessException("您要禁用的数据不存在");
            }

            entity.IsEnable = false;

            await seedDataTaskService.UpdateAsync(entity);

            return Ok();
        }
    }
}
