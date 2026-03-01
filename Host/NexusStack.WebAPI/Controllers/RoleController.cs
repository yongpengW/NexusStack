using Ardalis.Specification;
using Microsoft.AspNetCore.Mvc;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos.Roles;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Constants;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Utils;
using X.PagedList;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// 角色管理
    /// </summary>
    /// <param name="roleService"></param>
    /// <param name="userRoleService"></param>
    /// <param name="permissionService"></param>
    public class RoleController(IRoleService roleService,
        IUserRoleService userRoleService,
        IPermissionService permissionService) : BaseController
    {
        /// <summary>
        /// 获取角色分页数据
        /// </summary>
        /// <param name="platformType">所属平台(传0获取所有)</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("list/{platformType}"), NoLogging]
        public async Task<IPagedList<RoleDto>> GetListAsync(PlatformType platformType, [FromQuery] RoleQueryDto model)
        {
            var spec = Specifications<Role>.Create();
            spec.Query.OrderBy(a => a.Order);

            if (platformType > 0)
            {
                var platformStr = ((int)platformType).ToString();
                spec.Query.Where(a => a.Platforms == platformType);
            }

            // 当前用户角色不是超级管理员时，不允许查看超级管理员角色
            var roles = await roleService.GetListAsync(x => CurrentUser.Roles.Contains(x.Id));

            if (!roles.Any(x => x.Code == SystemRoleConstants.Root))
            {
                spec.Query.Where(a => a.Code != SystemRoleConstants.Root);
            }

            if (!string.IsNullOrEmpty(model.Keyword))
            {
                spec.Query.Search(a => a.Name, $"%{model.Keyword}%")
                    .Search(a => a.Code, $"%{model.Keyword}%")
                    .Search(a => a.Remark, $"%{model.Keyword}%");
            }

            if (model.IsEnable.HasValue)
            {
                spec.Query.Where(a => a.IsEnable == model.IsEnable.Value);
            }

            return await roleService.GetPagedListAsync<RoleDto>(spec, model.Page, model.Limit);
        }
    }
}
