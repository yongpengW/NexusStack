using Ardalis.Specification;
using LinqKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos.Users;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Core.Services.Users;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Enums;
using X.PagedList;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// 登录日志（当前用户）
    /// </summary>
    public class TokenLogController(IUserTokenService userTokenService) : BaseController
    {
        /// <summary>
        /// 获取当前用户的登录日志列表（分页，可按平台、时间筛选）
        /// </summary>
        /// <param name="model">分页及筛选：platformType、StartTime、EndTime</param>
        /// <returns></returns>
        [HttpGet("list"), NoLogging]
        public async Task<IPagedList<UserTokenLogDto>> GetLogListAsync([FromQuery] UserTokenQueryDto model)
        {
            var currentUser = base.CurrentUser;
            var spec = Specifications<UserToken>.Create();
            spec.Query.Where(ut => ut.UserId == CurrentUser.UserId);

            if (model.platformType > 0 && model.platformType != PlatformType.All)
            {
                spec.Query.Where(ut => ut.PlatformType == model.platformType);
            }

            if(model.StartTime.HasValue)
            {
                spec.Query.Where(ut => ut.CreatedAt >= model.StartTime.Value);
            }

            if (model.EndTime.HasValue)
            {
                spec.Query.Where(ut => ut.CreatedAt <= model.EndTime.Value);
            }

            spec.Query.Include(ut => ut.User);
            spec.Query.OrderByDescending(ut => ut.CreatedAt);

            return await userTokenService.GetPagedListAsync<UserTokenLogDto>(spec, model.Page, model.Limit);
        }
    }
}
