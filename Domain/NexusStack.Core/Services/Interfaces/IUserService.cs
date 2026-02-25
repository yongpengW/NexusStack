using NexusStack.Core.Entities.Users;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Interfaces
{
    public interface IUserService : IServiceBase<User>
    {
        /// <summary>
        /// 检查当前登录用户是否存在
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> CheckCurrentExists(CurrentUser user);

        /// <summary>
        /// 检查指定用户是否存在
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> CheckUserExists(long userId);

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        Task ResetPasswordAsync(long id);

        /// <summary>
        /// 根据用户获取所有有权限的门店列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        //Task<List<ShopDropDownDto>> GetAuthorizedStoreList();
    }
}
