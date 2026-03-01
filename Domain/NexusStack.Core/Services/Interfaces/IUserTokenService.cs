using NexusStack.Core.Dtos.Users;
using NexusStack.Core.Entities.Users;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Interfaces
{
    /// <summary>
    /// 用户token服务
    /// </summary>
    public interface IUserTokenService : IServiceBase<UserToken>
    {
        /// <summary>
        /// 生成图片验证码
        /// </summary>
        /// <returns></returns>
        Task<CaptchaDto> GenerateCaptchaAsync();

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="captchaCode"></param>
        /// <param name="captchaKey"></param>
        /// <returns></returns>
        Task<bool> ValidateCaptchaAsync(string captchaCode, string captchaKey);

        /// <summary>
        /// 验证用户Token，Token 无效或不存在时返回 null
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<UserTokenCacheDto?> ValidateTokenAsync(string token);

        /// <summary>
        /// 通过账号密码登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="platformType"></param>
        /// <returns></returns>
        Task<UserTokenDto> LoginWithPasswordAsync(string username, string password, PlatformType platformType);

        /// <summary>
        /// 使用 Refresh Token 获取新 Token
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        Task<UserTokenDto> RefreshTokenAsync(long userId, string refreshToken);
    }
}
