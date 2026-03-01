using LinqKit;
using Microsoft.AspNetCore.Mvc;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure.Models;
using NexusStack.Infrastructure.Utils;
using Ardalis.Specification;
using X.PagedList;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// 系统参数
    /// </summary>
    public class OptionController(IOptionsSerivce optionsService) : BaseController
    {
        /// <summary>
        /// 获取配置列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet, NoLogging]
        public async Task<IPagedList<OptionsDto>> GetListAsync([FromQuery] PagedQueryModelBase model)
        {
            var filter = PredicateBuilder.New<Options>(true);

            if (model.Keyword.IsNotNullOrEmpty())
            {
                filter = filter.And(a => a.Key.Contains(model.Keyword) || (a.Remark ?? "").Contains(model.Keyword) || a.Value.Contains(model.Keyword));
            }

            return await optionsService.GetPagedListAsync<OptionsDto>(filter, model.Page, model.Limit);
        }
    }
}
