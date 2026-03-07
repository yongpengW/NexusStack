using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.DbContexts;
using NexusStack.Infrastructure;

namespace NexusStack.Core.SeedData
{
    /// <summary>
    /// 初始化角色权限数据
    /// </summary>
    /// <param name="scopeFactory"></param>
    public class PermissionSeedData(IServiceScopeFactory scopeFactory) : ISeedData, ITransientDependency
    {
        public int Order => 5;

        public string ConfigPath { get; set; } = string.Empty;

        public async Task ApplyAsync(SeedDataTask model)
        {
            using var scope = scopeFactory.CreateScope();
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

            var data = new List<Permission>
            {
                new()
                {
                    Id = 2029546067980324866,
                    RoleId = 2029092668881113088,
                    MenuId = 2029131958226915328,
                    DataRange = DataRange.All
                },
                new()
                {
                    Id = 2029546067980324867,
                    RoleId = 2029092668881113088,
                    MenuId = 2029098077972992000,
                    DataRange = DataRange.All
                }
            };

            foreach (var item in data)
            {
                var exists = await permissionService.GetAsync(a => a.Id == item.Id);
                if (exists is null)
                {
                    await permissionService.InsertAsync(item);
                    continue;
                }

                exists.RoleId = item.RoleId;
                exists.MenuId = item.MenuId;
                exists.DataRange = item.DataRange;
                exists.IsDeleted = false;

                await permissionService.UpdateAsync(exists);
            }
        }
    }
}
