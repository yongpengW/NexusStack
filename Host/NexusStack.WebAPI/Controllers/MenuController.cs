using Ardalis.Specification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos;
using NexusStack.Core.Dtos.Menus;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Enums;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// 系统菜单管理
    /// </summary>
    public class MenuController(IMenuService menuService,
        IApiResrouceCoreService apiResourceService,
        IServiceBase<MenuResource> menuResourceService,
        IPermissionService permissionService,
        IUserRoleService userRoleService,
        IUserContextCacheService userContextCacheService) : BaseController
    {
        /// <summary>
        /// 获取菜单树
        /// </summary>
        /// <param name="platformType">平台类型</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("tree/{platformType}"), NoLogging]
        public async Task<List<MenuTreeDto>> GetTreeAsync(PlatformType platformType, [FromQuery] MenuTreeQueryDto model)
        {
            return await menuService.GetTreeListAsync(platformType, model);
        }

        /// <summary>
        /// 获取当前用户菜单树
        /// </summary>
        /// <param name="platformType">平台类型</param>
        /// <returns></returns>
        [HttpGet("usertree/{platformType}"), NoLogging]
        public async Task<List<MenuTreeDto>> GetUserTreeAsync(PlatformType platformType)
        {
            return await permissionService.GetUserMenuTreeListAsync(CurrentUser, platformType);
        }

        /// <summary>
        /// 菜单选择器列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("selector"), NoLogging]
        public async Task<List<SelectOptionDto>> GetMenuSelectorListAsync()
        {
            var spec = Specifications<Menu>.Create();
            spec.Query.Where(x => x.IsVisible && !x.IsDeleted && x.Type < MenuType.Operation).OrderBy(a => a.Order);

            var menus = await menuService.GetListAsync<MenuDto>(spec);

            var selectorList = new List<SelectOptionDto> { new() { label = "无", value = 0 } };
            selectorList.AddRange(menus.Select(x => new SelectOptionDto { label = x.Name, value = x.Id }));
            return selectorList;
        }

        /// <summary>
        /// 添加菜单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<long> PostAsync(CreateMenuDto model)
        {
            return await menuService.PostAsync(model);
        }

        /// <summary>
        /// 获取菜单详情
        /// </summary>
        /// <param name="id">菜单Id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public Task<MenuDto> GetByIdAsync(long id)
        {
            return menuService.GetByIdAsync<MenuDto>(id);
        }

        /// <summary>
        /// 修改菜单
        /// </summary>
        /// <param name="id">菜单Id</param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("{id}")]
        [OperationLogAction("修改菜单，菜单Id为:{id}，菜单Code为{model.Code}", "菜单管理")]
        public async Task<StatusCodeResult> PutAsync(long id, CreateMenuDto model)
        {
            await menuService.PutAsync(id, model);

            // 菜单属性（IsVisible / PlatformType 等）变更后，使持有该菜单权限的用户缓存失效
            await InvalidateMenuUsersAsync(id);

            return Ok();
        }

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("{id}")]
        [OperationLogAction("删除菜单，菜单Id为:{id}，菜单Code为{model.Code}", "菜单管理")]
        public async Task<StatusCodeResult> DeleteAsync(long id)
        {
            // 必须在删除前查询受影响用户（删除后 Permission 记录可能被级联清除）
            var affectedUserIds = await GetMenuAffectedUserIdsAsync(id);

            await menuService.DeleteAsync(id);

            foreach (var userId in affectedUserIds)
                await userContextCacheService.InvalidateAsync(userId);

            return Ok();
        }

        /// <summary>
        /// 获取菜单绑定接口
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/Resources"), NoLogging]
        public async Task<List<MenuResourceDto>> GetResourcesAsync(long id)
        {
            var resources = await apiResourceService.GetListAsync<ApiResourceDto>();

            var checkedList = await menuResourceService.GetListAsync(a => a.MenuId == id);

            var array = checkedList.Select(item => item.ApiResourceId);

            resources.ForEach(item => {
                if (array.Contains(item.Id))
                {
                    item.IsChecked = true;
                }
            });


            return resources.GroupBy(a => new { a.NameSpace, a.ControllerName, a.GroupName }).OrderBy(a => a.Key.NameSpace).Select(a =>
            {
                var resource = new MenuResourceDto
                {
                    Name = a.Key.GroupName,
                    Code = $"{a.Key.NameSpace}.{a.Key.ControllerName}",
                    Operations = a.Select(c => this.Mapper.Map<MenuResourceDto>(c)).ToList()
                };

                return resource;
            }).ToList();
        }

        /// <summary>
        /// 绑定接口
        /// </summary>
        /// <param name="id">菜单Id</param>
        /// <param name="resources">接口资源id数组</param>
        /// <returns></returns>
        [HttpPut("{id}/bind")]
        public async Task<StatusCodeResult> BindResourceAsync(long id, long[] resources)
        {
            var existLists = await menuResourceService.GetListAsync(a => a.MenuId == id);
            await menuResourceService.DeleteAsync(existLists);

            var newResources = resources.Select(a => new MenuResource
            {
                MenuId = id,
                ApiResourceId = a
            });

            await menuResourceService.InsertAsync(newResources);

            // 菜单绑定的 API 资源变更后，ApiPermissionKeys 缓存失效
            await InvalidateMenuUsersAsync(id);

            return Ok();
        }

        // ── 私有辅助方法 ──────────────────────────────────────────────────────────

        /// <summary>
        /// 查询持有指定菜单权限的所有用户 Id
        /// </summary>
        private async Task<List<long>> GetMenuAffectedUserIdsAsync(long menuId)
        {
            var roleIds = await permissionService.GetQueryable()
                .Where(p => p.MenuId == menuId)
                .Select(p => p.RoleId)
                .Distinct()
                .ToListAsync();

            if (roleIds.Count == 0)
                return new List<long>();

            return await userRoleService.GetQueryable()
                .Where(ur => roleIds.Contains(ur.RoleId))
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// 使持有指定菜单权限的所有用户缓存失效
        /// </summary>
        private async Task InvalidateMenuUsersAsync(long menuId)
        {
            var userIds = await GetMenuAffectedUserIdsAsync(menuId);
            foreach (var userId in userIds)
                await userContextCacheService.InvalidateAsync(userId);
        }
    }
}
