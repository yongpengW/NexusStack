using Ardalis.Specification;
using AutoMapper;
using NexusStack.Core.Dtos.Regions;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Excel.Export;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace NexusStack.Core.Services.SystemManagement
{
    public class RegionService(MainContext dbContext, IMapper mapper) : ServiceBase<Region>(dbContext, mapper), IRegionService, IScopedDependency
    {
        public override async Task<Region> InsertAsync(Region entity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(entity.IdSequences))
            {
                if (entity.ParentId == 0)
                {
                    entity.IdSequences = $".{entity.Id}.";
                }
                else
                {
                    var parent = await GetAsync(a => a.Id == entity.ParentId);
                    if (parent != null)
                    {
                        entity.IdSequences = $"{parent.IdSequences}{entity.Id}.";
                        if (parent.Level == RegionLevel.City)
                        {
                            throw new BusinessException("城市下不能创建下级行政区划");
                        }

                        entity.Level = parent.Level + 1;
                    }
                }
            }

            var existsCode = await GetAsync(a => a.Code == entity.Code);
            if (existsCode != null)
            {
                throw new BusinessException($"行政区划代码【{entity.Code}】已存在");
            }

            var existsNmae = await GetAsync(a => a.Name == entity.Name);
            //if (existsNmae != null)
            //{
            //    throw new BusinessException($"行政区划名称【{entity.Name}】已存在");
            //}

            await base.InsertAsync(entity, cancellationToken);

            return entity;
        }

        public async Task<List<RegionTreeDto>> GetTreeListAsync(RegionTreeQueryDto model)
        {
            var result = new List<RegionTreeDto>();
            var regions = await GetListAsync();

            var spec = Specifications<Region>.Create();

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
                    .Search(a => a.ShortName, $"%{model.Keyword}%")
                    .Search(a => a.Code, $"%{model.Keyword}%");

                flag = true;
            }


            var keywordRegions = await GetListAsync(spec);
            var count = keywordRegions.Count;

            while (flag)
            {
                var ids = keywordRegions.Select(x => x.Id).Distinct().ToList();
                var children = regions.Where(x => ids.Contains(x.ParentId));

                keywordRegions.AddRange(children);

                keywordRegions = keywordRegions.GroupBy(r => new { r.Id, r.Name, r.ParentId })
                                    .Select(g => g.First())
                                    .ToList();


                if (count == keywordRegions.Count)
                {
                    break;
                }

                count = keywordRegions.Count;
            }

            while (flag)
            {
                var parrentsIds = keywordRegions.Select(x => x.ParentId).Distinct().ToList();
                var parrents = regions.Where(x => parrentsIds.Contains(x.Id));

                keywordRegions.AddRange(parrents);

                keywordRegions = keywordRegions.GroupBy(r => new { r.Id, r.Name, r.ParentId })
                    .Select(g => g.First())
                    .ToList();

                if (count == keywordRegions.Count)
                {
                    break;
                }

                count = keywordRegions.Count;
            }


            List<RegionTreeDto> getTree(long parentId)
            {
                var children = keywordRegions.Where(a => a.ParentId == parentId).OrderBy(a => a.Order).ToList();
                return children.Select(a =>
                {
                    var dto = Mapper.Map<RegionTreeDto>(a);
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

            result = getTree(model.ParentId);

            return result;
        }
    }
}
