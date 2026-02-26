using Ardalis.Specification;
using AutoMapper;
using NexusStack.Core.Dtos.GlobalSettings;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using X.PagedList;

namespace NexusStack.Core.Services.SystemManagement
{
    public interface IGlobalSettingService : IServiceBase<GlobalSettings>
    {
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<IPagedList<GlobalSettingDto>> GetSettingListAsync(GlobalSettingQueryDto model);

        /// <summary>
        /// 获取模板
        /// </summary>
        /// <returns></returns>
        Task<GlobalSettingDto> GetDefaltSettingAsync();

        /// <summary>
        /// 获取编辑信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<GlobalSettingDto> GetEditGlobalSettingInfo(long id);

        /// <summary>
        /// Webapi获取GlobalSetting配置项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<GlobalSettingDto> GetSingleSettingForApiAsync(string key);

        /// <summary>
        /// 根据主键删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ErrorCodeException"></exception>
        Task<int> DeleteAsync(long id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 新增全局设置
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<long> PostAsync(CreateGlobalSettingDto model);

        /// <summary>
        /// 修改全局设置
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<int> PutAsync(CreateGlobalSettingDto model);
    }

    public class GlobalSettingService(MainContext dbContext, IMapper mapper) : ServiceBase<GlobalSettings>(dbContext, mapper), IGlobalSettingService, IScopedDependency
    {
        public async Task<IPagedList<GlobalSettingDto>> GetSettingListAsync(GlobalSettingQueryDto model)
        {
            //var spec = Specifications<GlobalSettings>.Create();
            //spec.Query.OrderByDescending(a => a.CreatedAt);
            //spec.Query.Where(w => w.AppId != "-1");
            //if (!string.IsNullOrEmpty(model.Keyword))
            //{
            //    spec.Query.Where(w => w.AppId.Contains(model.Keyword));
            //}

            //return await GetPagedListAsync<GlobalSettingDto>(spec, model.Page, model.Limit);

            return null;
        }

        public async Task<GlobalSettingDto> GetSingleSettingForApiAsync(string key)
        {
            //var spec = Specifications<GlobalSettings>.Create();
            //spec.Query.Where(w => w.Key == key && w.AppId == "0001");
            //return await GetAsync<GlobalSettingDto>(spec);
            return null;
        }

        public async Task<GlobalSettingDto> GetDefaltSettingAsync()
        {
            //var spec = Specifications<GlobalSettings>.Create();
            //spec.Query.Where(w => w.AppId == "-1");

            //return await GetAsync<GlobalSettingDto>(spec);
            return null;
        }

        public async Task<GlobalSettingDto> GetEditGlobalSettingInfo(long id)
        {
            var entity = await GetAsync(a => a.Id == id) ?? throw new BusinessException("数据不存在");
            var model = Mapper.Map<GlobalSettingDto>(entity);
            return model;
        }

        public async Task<long> PostAsync(CreateGlobalSettingDto model)
        {
            var entity = Mapper.Map<GlobalSettings>(model);
            await InsertAsync(entity);
            return entity.Id;
        }

        public async Task<int> PutAsync(CreateGlobalSettingDto model)
        {
            var entity = await GetAsync(a => a.Id == model.Id && a.Key == model.Key) ?? throw new BusinessException("你要修改的数据不存在");

            entity = Mapper.Map(model, entity);

            return await UpdateAsync(entity);
        }
    }
}
