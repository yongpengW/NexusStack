using AutoMapper;
using NexusStack.Core.Entities.Users;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using NexusStack.Infrastructure.Utils;
using StringExtensions = NexusStack.Infrastructure.Utils.StringExtensions;
using NexusStack.Core.Services.Interfaces;

namespace NexusStack.Core.Services.Users
{
    public class UserService(MainContext dbContext, IMapper mapper, CurrentUser currentUser) : ServiceBase<User>(dbContext, mapper), IUserService, IScopedDependency
    {
        public async Task<bool> CheckCurrentExists(CurrentUser user)
        {
            return await GetLongCountAsync(x => x.Id == user.UserId) > 0;
        }

        public async Task<bool> CheckUserExists(long userId)
        {
            return await dbContext.Set<User>().IgnoreQueryFilters().AnyAsync(x => x.Id == userId);
        }

        public override async Task<User> InsertAsync(User entity, CancellationToken cancellationToken = default)
        {
            // 密码不为空的时候，加密密码
            if (entity.Password.IsNotNullOrEmpty())
            {
                // 为每个密码生成一个32位的唯一盐值
                entity.PasswordSalt = StringExtensions.GeneratePassworldSalt();

                entity.Password = entity.Password.EncodePassword(entity.PasswordSalt);
            }

            if (entity.UserName.IsNotNullOrEmpty() && await ExistsAsync(item => item.UserName == entity.UserName))
            {
                throw new BusinessException("此用户名已存在");
            }

            if (entity.Mobile.IsNotNullOrEmpty() && await ExistsAsync(item => item.Mobile == entity.Mobile))
            {
                throw new BusinessException("此手机号码已存在");
            }

            if (!entity.Email.IsNullOrEmpty() && await ExistsAsync(a => a.Email == entity.Email))
            {
                throw new BusinessException("此邮箱已存在");
            }

            await base.InsertAsync(entity, cancellationToken);

            // 发送设置密码的短信
            return entity;
        }

        /// <summary>
        /// 重置密码（重置为手机号后 6 位）
        /// </summary>
        /// <param name="id"></param>
        public async Task ResetPasswordAsync(long id)
        {
            var user = await GetByIdAsync(id);

            if (user == null)
            {
                throw new BusinessException("用户不存在");
            }

            if (user.Mobile.IsNullOrEmpty())
            {
                throw new BusinessException("请先为用户设置手机号码");
            }

            if (user.Mobile!.Length < 6)
            {
                throw new BusinessException("用户手机号长度不足 6 位，无法重置密码");
            }

            // 重置为手机号后 6 位，与创建时默认密码策略一致
            user.PasswordSalt = StringExtensions.GeneratePassworldSalt();
            user.Password = user.Mobile[^6..].EncodePassword(user.PasswordSalt);

            await UpdateAsync(user);
        }


    }
}
