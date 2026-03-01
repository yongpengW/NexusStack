using Ardalis.Specification;
using Microsoft.AspNetCore.Mvc;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos;
using NexusStack.Core.Dtos.Permissions;
using NexusStack.Core.Dtos.Roles;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Constants;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
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
            var roles = await roleService.GetListAsync(x => CurrentUser.RoleIds.Contains(x.Id));

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

        /// <summary>
        /// 角色选择器列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("selector"), NoLogging]
        public List<SelectOptionDto> GetRoleSelectorListAsync()
        {
            var spec = Specifications<Role>.Create();
            spec.Query.Where(x => x.IsEnable).OrderBy(a => a.Order);
            var roles = roleService.GetListAsync<RoleDto>(spec).Result;
            var selectorList = new List<SelectOptionDto>();
            selectorList.AddRange(roles.Select(x => new SelectOptionDto
            {
                label = x.Name,
                value = x.Id
            }).ToList());
            return selectorList;
        }

        /// <summary>
        /// 获取角色信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}"), NoLogging]
        public Task<RoleDto> GetByIdAsync(long id)
        {
            return roleService.GetByIdAsync<RoleDto>(id);
        }

        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<long> PostAsync(CreateRoleDto model)
        {
            var entity = this.Mapper.Map<Role>(model);
            await roleService.InsertAsync(entity);
            return entity.Id;
        }

        /// <summary>
        /// 修改角色信息
        /// </summary>
        /// <param name="id">角色Id</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<StatusCodeResult> PutAsync(long id, CreateRoleDto model)
        {
            var entity = await roleService.GetAsync(a => a.Id == id);
            if (entity is null)
            {
                throw new BusinessException("你要修改的数据不存在");
            }

            if (!model.IsEnable)
            {
                var userroles = await userRoleService.GetLongCountAsync(a => a.RoleId == id);
                if (userroles > 0)
                {
                    throw new BusinessException("该角色正在使用中，无法禁用");
                }
            }

            entity = this.Mapper.Map(model, entity);

            await roleService.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// 启用角色
        /// </summary>
        /// <param name="id">角色Id</param>
        /// <returns></returns>
        [HttpPut("enable/{id}")]
        public async Task<StatusCodeResult> EnableAsync(long id)
        {
            var entity = await roleService.GetAsync(a => a.Id == id);
            if (entity is null)
            {
                throw new BusinessException("你要启用的数据不存在");
            }

            entity.IsEnable = true;
            await roleService.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// 禁用角色
        /// </summary>
        /// <param name="id">角色Id</param>
        /// <returns></returns>
        [HttpPut("disable/{id}")]
        public async Task<StatusCodeResult> DisableAsync(long id)
        {
            var entity = await roleService.GetAsync(a => a.Id == id);
            if (entity is null)
            {
                throw new BusinessException("你要禁用的数据不存在");
            }

            var userroles = await userRoleService.GetLongCountAsync(a => a.RoleId == id);
            if (userroles > 0)
            {
                throw new BusinessException("该角色正在使用中，无法禁用");
            }

            if (entity.IsSystem)
            {
                throw new BusinessException("禁止禁用系统内置角色");
            }

            entity.IsEnable = false;
            await roleService.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id">角色Id</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<StatusCodeResult> DeleteAsync(long id)
        {
            var entity = await roleService.GetAsync(a => a.Id == id);
            if (entity is null)
            {
                throw new BusinessException("你要删除的数据不存在");
            }

            var userroles = await userRoleService.GetLongCountAsync(a => a.RoleId == id);
            if (userroles > 0)
            {
                throw new BusinessException("该角色下存在用户，无法删除");
            }

            if (entity.IsSystem)
            {
                throw new BusinessException("禁止删除系统内置角色");
            }

            await roleService.DeleteAsync(entity);
            return Ok();
        }

        /// <summary>
        /// 获取角色权限
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("permission"), NoLogging]
        public Task<List<PermissionDto>> GetRoleAsync([FromQuery] PermissionQueryDto model)
        {
            return permissionService.GetRolePermissionAsync(model.RoleId, model.PlatformType);
        }

        /// <summary>
        /// 修改角色权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="menus">菜单id数组</param>
        /// <returns></returns>
        [HttpPost("permission/{roleId}")]
        public async Task<StatusCodeResult> PostAsync(long roleId, ChangeRolePermissionDto dto)
        {
            await permissionService.ChangeRolePermissionAsync(dto);
            return Ok();
        }
    }
}
