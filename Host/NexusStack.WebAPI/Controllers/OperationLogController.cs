using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure.Utils;
using X.PagedList;
using X.PagedList.Extensions;
using Ardalis.Specification;

namespace NexusStack.WebAPI.Controllers
{
    public class OperationLogController(IOperationLogService operationLogService, 
        IUserService userService) : BaseController
    {
        /// <summary>
        /// 操作日志列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("list"), NoLogging]
        public async Task<IPagedList<OperationLogDto>> GetListAsync([FromQuery] OperationLogQueryDto model)
        {
            var filter = PredicateBuilder.New<OperationLog>(true);

            if (model.Keyword.IsNotNullOrEmpty())
            {
                filter.Or(a => a.OperationContent.Contains(model.Keyword));
                filter.Or(a => a.OperationMenu.Contains(model.Keyword));
            }

            if (model.StartTime.HasValue)
            {
                filter.And(a => a.CreatedAt >= model.StartTime.Value);
            }

            if (model.EndTime.HasValue)
            {
                filter.And(a => a.CreatedAt < model.EndTime.Value.AddDays(1));
            }
            if (model.LogType.HasValue)
            {
                filter.And(a => a.LogType == model.LogType);
            }
            if (!string.IsNullOrEmpty(model.MenuCode))
            {
                filter.And(a => a.MenuCode.Contains(model.MenuCode));
            }
            if (model.UserId.HasValue)
            {
                filter.And(a => a.CreatedBy == model.UserId);
            }

            var query = from log in operationLogService.GetExpandable().Where(filter)
                        join user in userService.GetQueryable() on log.CreatedBy equals user.Id into lu
                        from s in lu.DefaultIfEmpty()
                        select new OperationLogDto
                        {
                            Id = log.Id,
                            CreatedAt = log.CreatedAt,
                            CreatedBy = s.RealName ?? string.Empty,
                            OperationContent = log.OperationContent,
                            OperationMenu = log.OperationMenu,
                            IpAddress = log.IpAddress ?? string.Empty,
                            UserAgent = log.UserAgent ?? string.Empty,
                            RequestJson = log.Remark ?? string.Empty,
                            LogType = log.LogType.GetDescription(),

                        };
            query = query.OrderByDescending(a => a.Id);

            return query.ToPagedList(model.Page, model.Limit);
        }
        /// <summary>
        /// 导出到excel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("export")]
        public async Task<IActionResult> export([FromQuery] OperationLogQueryDto model)
        {
            var filter = PredicateBuilder.New<OperationLog>(true);

            if (model.Keyword.IsNotNullOrEmpty())
            {
                filter.Or(a => a.OperationContent.Contains(model.Keyword));
                filter.Or(a => a.OperationMenu.Contains(model.Keyword));
            }

            if (model.StartTime.HasValue)
            {
                filter.And(a => a.CreatedAt >= model.StartTime.Value);
            }

            if (model.EndTime.HasValue)
            {
                filter.And(a => a.CreatedAt < model.EndTime.Value.AddDays(1));
            }
            if (model.LogType.HasValue)
            {
                filter.And(a => a.LogType == model.LogType);
            }
            if (!string.IsNullOrEmpty(model.MenuCode))
            {
                filter.And(a => a.OperationMenu.Contains(model.MenuCode));
            }
            if (model.UserId.HasValue)
            {
                filter.And(a => a.CreatedBy == model.UserId);
            }

            var query = from log in operationLogService.GetExpandable().Where(filter)
                        join user in userService.GetQueryable() on log.CreatedBy equals user.Id into lu
                        from s in lu.DefaultIfEmpty()
                        select new OperationLogDto
                        {
                            Id = log.Id,
                            CreatedAt = log.CreatedAt,
                            CreatedBy = s.RealName ?? string.Empty,
                            OperationContent = log.OperationContent,
                            OperationMenu = log.OperationMenu,
                            IpAddress = log.IpAddress ?? string.Empty,
                            UserAgent = log.UserAgent ?? string.Empty,
                            RequestJson = log.Remark ?? string.Empty,
                            LogType = log.LogType.GetDescription(),

                        };
            query = query.OrderByDescending(a => a.Id);

            var data = query.ToList();
            var bytes = operationLogService.ExportLogAsync(data);
            var ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return new FileStreamResult(new MemoryStream(bytes), ContentType) { FileDownloadName = "OperationLogs.xlsx" };
        }
    }
}
