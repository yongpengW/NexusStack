using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Entities.Users;
using NexusStack.EFCore.DbContexts;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;

namespace NexusStack.Core.SeedData
{
    /// <summary>
    /// 初始化角色数据
    /// </summary>
    /// <param name="scopeFactory"></param>
    public class RoleSeedData(IServiceScopeFactory scopeFactory) : ISeedData, ITransientDependency
    {
        public int Order => 2;

        public string ConfigPath { get; set; } = string.Empty;

        public async Task ApplyAsync(SeedDataTask model)
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MainContext>();

            var data = new List<Role>
            {
                new()
                {
                    Id = 1,
                    Name = "超级管理员",
                    Platforms = PlatformType.Pc,
                    Code = "ROOT",
                    IsSystem = true,
                    Order = 1,
                    IsEnable = true,
                    SystemId = 0
                },
                new()
                {
                    Id = 2029092668881113088,
                    Name = "员工",
                    Platforms = PlatformType.Pc,
                    Code = "staff",
                    IsSystem = false,
                    Order = 99,
                    IsEnable = true,
                    SystemId = 0,
                    Remark = "测试"
                }
            };

            foreach (var item in data)
            {
                var exists = await dbContext.Set<Role>().IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == item.Id);
                if (exists is null)
                {
                    await dbContext.Set<Role>().AddAsync(item);
                    continue;
                }

                exists.Name = item.Name;
                exists.Platforms = item.Platforms;
                exists.Code = item.Code;
                exists.IsSystem = item.IsSystem;
                exists.Order = item.Order;
                exists.IsEnable = item.IsEnable;
                exists.SystemId = item.SystemId;
                exists.IsDeleted = false;
                exists.Remark = item.Remark;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
