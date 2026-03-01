using Ardalis.Specification;
using Microsoft.AspNetCore.Mvc;
using NexusStack.Core.Attributes;
using NexusStack.Core.Dtos;
using NexusStack.Core.Dtos.Regions;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.Utils;

namespace NexusStack.WebAPI.Controllers
{
    /// <summary>
    /// 行政区域管理
    /// </summary>
    public class RegionController(IRegionService regionService) : BaseController
    {
        /// <summary>
        /// 获取区域列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("list"), NoLogging]
        public Task<List<RegionDto>> GetListAsync([FromQuery] RegionQueryDto model)
        {
            var spec = Specifications<Region>.Create();
            spec.Query.OrderBy(a => a.Order);

            if (!model.Keyword.IsNullOrEmpty())
            {
                spec.Query.Search(a => a.Name, $"%{model.Keyword}%")
                          .Search(a => a.ShortName, $"%{model.Keyword}%")
                          .Search(a => a.Code, $"%{model.Keyword}%");
            }

            //if (model.ParentId == 0 && model.IsCurrent)
            //    spec.Query.Where(a => a.Id == this.CurrentUser.RegionId);
            //else
            //    spec.Query.Where(a => a.ParentId == model.ParentId);

            return regionService.GetListAsync<RegionDto>(spec);
        }

        /// <summary>
        /// 获取区域树
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("tree"), NoLogging]
        public async Task<List<RegionTreeDto>> GetTreeAsync([FromQuery] RegionTreeQueryDto model)
        {
            return await regionService.GetTreeListAsync(model);
        }

        /// <summary>
        /// 地区部门选择器，可以选择区域，部门，店铺
        /// </summary>
        /// <param name="level">等级</param>
        /// <param name="isIncludeZero">是否包含'无'</param>
        /// <returns></returns>
        [HttpGet("selector"), NoLogging]
        public async Task<List<SelectOptionDto>> GetRegionSelectorListAsync(RegionLevel? level, bool isIncludeZero = true)
        {
            var spec = Specifications<Region>.Create();
            spec.Query.Where(x => x.IsEnable && !x.IsDeleted);

            if (level.HasValue)
            {
                spec.Query.Where(x => x.Level == level.Value);
            }

            var regions = await regionService.GetListAsync<RegionDto>(spec);

            var selectorList = new List<SelectOptionDto>();

            if (isIncludeZero)
            {
                selectorList.Add(new SelectOptionDto
                {
                    label = "无",
                    value = 0
                });
            }


            selectorList.AddRange(regions.Select(x => new SelectOptionDto
            {
                label = x.Name,
                value = x.Id
            }).ToList());

            return selectorList;
        }

        /// <summary>
        /// 获取区域部门树选择器
        /// </summary>
        /// <param name="isIncludeShop">是否包含门店</param>
        /// <returns></returns>
        //[HttpGet("treeSelector"), NoLogging]
        //public async Task<List<SelectOptionDto>> GetRegionTreeSelectorAsync(bool isIncludeShop = true)
        //{
        //    var regions = await regionService.GetListAsync();
        //    var shops = await shopService.GetListAsync(x => x.State);

        //    List<SelectOptionDto> getChildren(long parentId)
        //    {
        //        var children = regions.Where(x => x.ParentId == parentId).OrderBy(a => a.Order).ToList();
        //        return children.Select(x =>
        //        {
        //            var item = new SelectOptionDto
        //            {
        //                label = x.Name,
        //                value = x.Id,
        //                children = getChildren(x.Id)
        //            };

        //            if (isIncludeShop)
        //            {
        //                var shopList = shops.Where(a => a.RegionId == x.Id).ToList();
        //                if (shopList.Count > 0)
        //                {
        //                    item.children.AddRange(shopList.Select(a => new SelectOptionDto
        //                    {
        //                        label = a.ShopName,
        //                        value = a.Id
        //                    }).ToList());
        //                }

        //            }

        //            return item;

        //        }).ToList();
        //    }


        //    return getChildren(0);
        //}

        /// <summary>
        /// 添加行政区划
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<long> PostAsync(CreateRegionDto model)
        {
            var entity = this.Mapper.Map<Region>(model);

            await regionService.InsertAsync(entity);
            return entity.Id;
        }

        /// <summary>
        /// 修改行政区域
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        [HttpPut("{id}")]
        public async Task<StatusCodeResult> PutAsync(long id, CreateRegionDto model)
        {
            var entity = await regionService.GetAsync(a => a.Id == id);
            if (entity is null)
            {
                throw new BusinessException("你要修改的数据不存在");
            }

            //如果填入的父级Id是当前区域Id，则将父级Id设置为0
            if (entity.Id == model.ParentId)
            {
                model.ParentId = 0;
            }

            entity = this.Mapper.Map(model, entity);

            await regionService.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// 获取区域信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}"), NoLogging]
        public async Task<RegionDto> GetByIdAsync(long id)
        {
            return await regionService.GetAsync<RegionDto>(a => a.Id == id);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        [HttpDelete("{id}")]
        public async Task<StatusCodeResult> DeleteAsync(long id)
        {

            var entity = await regionService.GetByIdAsync(id);
            if (entity is null)
            {
                throw new BusinessException("你要删除的数据不存在");
            }

            var children = await regionService.GetListAsync(a => a.ParentId == id);
            if (children.Count > 0)
            {
                throw new BusinessException("请先删除子级区域");
            }

            await regionService.DeleteAsync(entity);
            return Ok();
        }

        /// <summary>
        /// 启用该区域
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("Enable/{id}")]
        public async Task<StatusCodeResult> ItemEnableAsync(long id)
        {
            var entity = await regionService.GetByIdAsync(id);

            if (entity is null)
            {
                throw new BusinessException("您要启用的数据不存在");
            }

            entity.IsEnable = true;

            await regionService.UpdateAsync(entity);

            return Ok();
        }

        /// <summary>
        /// 禁用该区域
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("Disable/{id}")]
        public async Task<StatusCodeResult> ItemDisableAsync(long id)
        {
            var entity = await regionService.GetByIdAsync(id);

            if (entity is null)
            {
                throw new BusinessException("您要禁用的数据不存在");
            }

            entity.IsEnable = false;

            await regionService.UpdateAsync(entity);

            return Ok();
        }
    }
}
