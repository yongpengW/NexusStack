using Microsoft.AspNetCore.Mvc;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos.GlobalSettings;
using NexusStack.Core.Services.SystemManagement;
using NexusStack.Redis;
using X.PagedList;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// 系统全局设置
    /// </summary>
    /// <param name="globalSettingService"></param>
    public class GlobalSettingController(IGlobalSettingService globalSettingService) : BaseController
    {
        /// <summary>
        /// 获取全局设置列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("list"), NoLogging]
        public async Task<IPagedList<GlobalSettingDto>> List([FromQuery] GlobalSettingQueryDto model)
        {
            return await globalSettingService.GetSettingListAsync(model);
        }

        /// <summary>
        /// 获取设置模板
        /// </summary>
        /// <returns></returns>
        [HttpGet("getDefaultSetting"), NoLogging]
        public async Task<GlobalSettingDto> GetDefaultSetting()
        {
            return await globalSettingService.GetDefaltSettingAsync();
        }

        /// <summary>
        /// 根据Id获取设置信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ErrorCodeException"></exception>
        [HttpGet("getEditGlobalSettingInfoById"), NoLogging]
        public async Task<GlobalSettingDto> GetEditGlobalSettingInfo(long id)
        {
            return await globalSettingService.GetEditGlobalSettingInfo(id);
        }

        /// <summary>
        /// 删除全局设置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ErrorCodeException"></exception>
        [HttpDelete("delete")]
        public async Task<long> Delete([FromQuery] long id)
        {
            return await globalSettingService.DeleteAsync(id);
        }

        /// <summary>
        /// 添加全局设置
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ErrorCodeException"></exception>
        [HttpPost("addSetting")]
        public async Task<long> PostAsync(CreateGlobalSettingDto model)
        {
            return await globalSettingService.PostAsync(model);
        }

        /// <summary>
        /// 修改全局设置
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ErrorCodeException"></exception>
        [HttpPut("updateSetting")]
        public async Task<long> PutAsync(CreateGlobalSettingDto model)
        {
            return await globalSettingService.PutAsync(model);
        }
    }
}
