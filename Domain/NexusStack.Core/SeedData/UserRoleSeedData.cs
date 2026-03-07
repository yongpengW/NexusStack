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
    /// 初始化用户角色关联数据
    /// </summary>
    /// <param name="scopeFactory"></param>
    public class UserRoleSeedData(IServiceScopeFactory scopeFactory) : ISeedData, ITransientDependency
    {
        public int Order => 6;

        public string ConfigPath { get; set; } = string.Empty;

        public async Task ApplyAsync(SeedDataTask model)
        {
            using var scope = scopeFactory.CreateScope();
            var userRoleService = scope.ServiceProvider.GetRequiredService<IUserRoleService>();

            var data = new List<UserRole>
            {
                new()
                {
                    Id = 2029562242202210304,
                    UserId = 1,
                    RoleId = 1
                },
                new()
                {
                    Id = 2029941533251342336,
                    UserId = 2029562151177424896,
                    RoleId = 1
                }
            };

            foreach (var item in data)
            {
                var exists = await userRoleService.GetAsync(a => a.Id == item.Id);
                if (exists is null)
                {
                    await userRoleService.InsertAsync(item);
                    continue;
                }

                exists.UserId = item.UserId;
                exists.RoleId = item.RoleId;
                exists.IsDeleted = false;

                await userRoleService.UpdateAsync(item);
            }
        }
    }
}
