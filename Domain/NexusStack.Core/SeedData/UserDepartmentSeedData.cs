using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Entities.Users;
using NexusStack.Core.Services.Users;
using NexusStack.EFCore.DbContexts;
using NexusStack.Infrastructure;

namespace NexusStack.Core.SeedData
{
    /// <summary>
    /// 初始化用户组织关联数据
    /// </summary>
    /// <param name="scopeFactory"></param>
    public class UserDepartmentSeedData(IServiceScopeFactory scopeFactory) : ISeedData, ITransientDependency
    {
        public int Order => 7;

        public string ConfigPath { get; set; } = string.Empty;

        public async Task ApplyAsync(SeedDataTask model)
        {
            using var scope = scopeFactory.CreateScope();
            var userDepartmentService = scope.ServiceProvider.GetRequiredService<IUserDepartmentService>();

            var data = new List<UserDepartment>
            {
                new()
                {
                    Id = 2029562242395148288,
                    UserId = 1,
                    DepartmentId = 2028832689913729024
                },
                new()
                {
                    Id = 2029941533796601856,
                    UserId = 2029562151177424896,
                    DepartmentId = 2028832689913729024
                }
            };

            foreach (var item in data)
            {
                var exists = await userDepartmentService.GetAsync(a => a.Id == item.Id);
                if (exists is null)
                {
                    await userDepartmentService.InsertAsync(item);
                    continue;
                }

                exists.UserId = item.UserId;
                exists.DepartmentId = item.DepartmentId;
                exists.IsDeleted = false;

                await userDepartmentService.UpdateAsync(exists);
            }
        }
    }
}
