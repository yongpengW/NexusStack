using AutoMapper;
using NexusStack.Core.Dtos.Menus;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
using Ardalis.Specification;
using System;
using System.Collections.Generic;
using System.Text;
using NexusStack.Core.Services.Interfaces;

namespace NexusStack.Core.Services.SystemManagement
{
    public class MenuService(MainContext dbContext, IMapper mapper) : ServiceBase<Menu>(dbContext, mapper), IMenuService, IScopedDependency
    {
        public async Task<int> DeleteAsync(long id)
        {
            var entity = await GetAsync(a => a.Id == id);

            if (entity is null)
            {
                throw new BusinessException("你要删除的数据不存在");
            }

            var children = await GetListAsync(a => a.ParentId == id);
            if (children.Count > 0)
            {
                throw new BusinessException("请先删除子级菜单");
            }

            return await DeleteAsync(entity);
        }

        public override async Task<Menu> InsertAsync(Menu entity, CancellationToken cancellationToken = default)
        {
            if (entity.ParentId != 0)
            {
                var parent = await GetByIdAsync(entity.ParentId) ?? throw new BusinessException("父级菜单不存在");
                entity.IdSequences = $"{parent.IdSequences}{entity.Id}.";
            }
            else
            {
                entity.IdSequences = $".{entity.Id}.";
            }

            var exists = await GetAsync(a => a.Code == entity.Code);
            if (exists != null && exists.Id != entity.Id)
            {
                throw new BusinessException("菜单代码已存在");
            }

            if (entity.Id == entity.ParentId)
            {
                throw new BusinessException("菜单的父级菜单不能是自己");
            }

            return await base.InsertAsync(entity, cancellationToken);
        }

        public async Task<List<MenuTreeDto>> GetTreeListAsync(PlatformType platformType, MenuTreeQueryDto model)
        {
            //if (!model.ParentCode.IsNullOrEmpty())
            //{
            //    var parent = await this.GetAsync(a => a.Code == model.ParentCode && a.PlatformType == platformType) ?? throw new ErrorCodeException(-1, $"未找到代码【{model.ParentCode}】对应菜单") ;
            //    model.ParentId = parent.Id;
            //}

            var menus = await GetListAsync();

            var spec = Specifications<Menu>.Create();

            if (platformType != PlatformType.All)
            {
                spec.Query.Where(a => a.PlatformType == platformType);
            }

            if (model.ParentId != 0)
            {
                if (model.IncludeChilds)
                {
                    spec.Query.Search(a => a.IdSequences, $"%{model.ParentId}%");
                }
                else
                {
                    spec.Query.Where(a => a.ParentId == model.ParentId);
                }
            }

            bool flag = false;

            if (!string.IsNullOrEmpty(model.Keyword))
            {
                spec.Query.Search(a => a.Name, $"%{model.Keyword}%")
                    .Search(a => a.Code, $"%{model.Keyword}%");

                flag = true;
            }

            var keywordMenus = await GetListAsync(spec);
            var count = keywordMenus.Count;

            while (flag)
            {
                var ids = keywordMenus.Select(x => x.Id).Distinct().ToList();
                var children = menus.Where(x => ids.Contains(x.ParentId));
                keywordMenus.AddRange(children);

                keywordMenus = keywordMenus.GroupBy(r => new { r.Id, r.Name, r.ParentId })
                    .Select(g => g.First())
                    .ToList();

                if (count == keywordMenus.Count)
                {
                    break;
                }

                count = keywordMenus.Count;
            }

            List<MenuTreeDto> getTree(long parentId)
            {
                var children = keywordMenus.Where(a => a.ParentId == parentId).OrderBy(a => a.Order).ToList();
                return children.Select(a =>
                {
                    var dto = Mapper.Map<MenuTreeDto>(a);
                    if (model.IncludeChilds)
                    {
                        dto.Children = getTree(a.Id);
                    }

                    if (dto.Children.Count == 0)
                    {
                        dto.Children = null;
                    }

                    return dto;
                }).ToList();
            }

            return getTree(model.ParentId);
        }

        public async Task<long> PostAsync(CreateMenuDto model)
        {
            var entity = Mapper.Map<Menu>(model);
            await InsertAsync(entity);
            return entity.Id;
        }

        public async Task<int> PutAsync(long id, CreateMenuDto model)
        {
            var entity = await GetAsync(a => a.Id == id) ?? throw new BusinessException("你要修改的数据不存在");
            entity = Mapper.Map(model, entity);

            return await UpdateAsync(entity);
        }

        /// <summary>
        /// 根据 Code 获取父级菜单
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public virtual async Task<List<Menu>> GetParentMenuAsync(string menuCode)
        {
            var menu = await GetAsync(a => a.Code == menuCode);
            return await GetParentMenuAsync(menu);
        }

        /// <summary>
        /// 根据菜单对象获取父级菜单
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public virtual async Task<List<Menu>> GetParentMenuAsync(Menu menu)
        {
            if (menu == null)
            {
                return new List<Menu>();
            }

            var parentIds = menu.IdSequences.Split(".", StringSplitOptions.RemoveEmptyEntries).Select(a => Convert.ToInt64(a));
            var parents = await GetListAsync(a => parentIds.Contains(a.Id));
            return parents;
        }
    }
}
