using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexusStack.Core.Entities.Schedules;
using NexusStack.Core.Entities.Users;
using NexusStack.EFCore.DbContexts;
using NexusStack.Infrastructure;

namespace NexusStack.Core.SeedData
{
    /// <summary>
    /// 初始化用户数据
    /// </summary>
    /// <param name="scopeFactory"></param>
    public class UserSeedData(IServiceScopeFactory scopeFactory) : ISeedData, ITransientDependency
    {
        public int Order => 3;

        public string ConfigPath { get; set; } = string.Empty;

        public async Task ApplyAsync(SeedDataTask model)
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MainContext>();

            var data = new List<User>
            {
                new()
                {
                    Id = 1,
                    Mobile = "13256873823",
                    RealName = "Leo Wang",
                    UserName = "admin",
                    NickName = "Leo",
                    Password = "jrKi7/1uKUMH5GVGmUMKS+xLRjZ+RXYHt3cjWTAYe0k=",
                    PasswordSalt = "qc6SX81B0DvBE32FnldeLt4UvMUlOGshbJTSOnUbZ9E=",
                    IsEnable = true,
                    Gender = Gender.Male,
                    Avatar = null,
                    Email = "leowang.interesting@gmail.com",
                    LastLoginTime = DateTimeOffset.Parse("2026-03-06 13:36:36.320411+08:00"),
                    SignatureUrl = null
                },
                new()
                {
                    Id = 2029562151177424896,
                    Mobile = "17854213431",
                    RealName = "王勇彭",
                    UserName = "leowang",
                    NickName = "Leo",
                    Password = "0LR+hUWEPJDdzDVbe0b1XEWIlyPr9TKvosi5VlXiK40=",
                    PasswordSalt = "BKPMh40hR9IXWYI9lSlAu1L9JmLn5TMn2p5x8dAkHXc=",
                    IsEnable = true,
                    Gender = Gender.Male,
                    Avatar = null,
                    Email = "67603960@qq.com",
                    LastLoginTime = DateTimeOffset.Parse("2026-03-07 10:40:51.087365+08:00"),
                    SignatureUrl = null
                }
            };

            foreach (var item in data)
            {
                var exists = await dbContext.Set<User>().IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == item.Id);
                if (exists is null)
                {
                    await dbContext.Set<User>().AddAsync(item);
                    continue;
                }

                exists.Mobile = item.Mobile;
                exists.RealName = item.RealName;
                exists.UserName = item.UserName;
                exists.NickName = item.NickName;
                exists.Password = item.Password;
                exists.PasswordSalt = item.PasswordSalt;
                exists.IsEnable = item.IsEnable;
                exists.Gender = item.Gender;
                exists.Avatar = item.Avatar;
                exists.Email = item.Email;
                exists.LastLoginTime = item.LastLoginTime;
                exists.SignatureUrl = item.SignatureUrl;
                exists.IsDeleted = false;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
