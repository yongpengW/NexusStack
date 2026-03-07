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
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

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
                    LastLoginTime = DateTimeOffset.UtcNow,
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
                    LastLoginTime = DateTimeOffset.UtcNow,
                    SignatureUrl = null
                }
            };

            foreach (var item in data)
            {
                var exists = await userService.GetAsync(a => a.Id == item.Id);
                if (exists is null)
                {
                    await userService.InsertAsync(item);
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

                await userService.UpdateAsync(exists);
            }
        }
    }
}
