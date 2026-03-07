using Microsoft.Extensions.DependencyInjection;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Services.Interfaces;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;

namespace NexusStack.Core.SeedData
{
    /// <summary>
    /// 初始化基础行政区划数据
    /// </summary>
    /// <param name="scopeFactory"></param>
    public class RegionSeedData(IServiceScopeFactory scopeFactory) : ISeedData, ITransientDependency
    {
        public int Order => 1;

        public string ConfigPath { get; set; } = string.Empty;

        public async Task ApplyAsync(SeedDataTask model)
        {
            using var scope = scopeFactory.CreateScope();
            var regionService = scope.ServiceProvider.GetRequiredService<IRegionService>();

            var regions = new List<Region>
            {
                new()
                {
                    Id = 2028823956101926912,
                    Name = "中国",
                    ShortName = "中国",
                    Code = "CHINA",
                    ParentId = 0,
                    Level = RegionLevel.Country,
                    Order = 1,
                    IdSequences = ".2028823956101926912.",
                    IsEnable = true
                },
                new()
                {
                    Id = 2028824300970184704,
                    Name = "山东",
                    ShortName = "山东",
                    Code = "SHANDONG",
                    ParentId = 2028823956101926912,
                    Level = RegionLevel.Province,
                    Order = 1,
                    IdSequences = ".2028823956101926912.2028824300970184704.",
                    IsEnable = true,
                    Remark = "你好"
                },
                new()
                {
                    Id = 2028832689913729024,
                    Name = "青岛",
                    ShortName = "青岛",
                    Code = "QINGDAO",
                    ParentId = 2028824300970184704,
                    Level = RegionLevel.City,
                    Order = 1,
                    IdSequences = ".2028823956101926912.2028824300970184704.2028832689913729024.",
                    IsEnable = true
                }
            };

            foreach (var item in regions)
            {
                var exists = await regionService.GetAsync(a => a.Code == item.Code);
                if (exists is null)
                {
                    await regionService.InsertAsync(item);
                    continue;
                }

                exists.Name = item.Name;
                exists.ShortName = item.ShortName;
                exists.ParentId = item.ParentId;
                exists.Level = item.Level;
                exists.Order = item.Order;
                exists.IdSequences = item.IdSequences;
                exists.IsEnable = item.IsEnable;
                exists.Remark = item.Remark;

                await regionService.UpdateAsync(exists);
            }
        }
    }
}
