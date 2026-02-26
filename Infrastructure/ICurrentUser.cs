using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace NexusStack.Infrastructure
{
    public interface ICurrentUser
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        long UserId { get; }

        /// <summary>
        /// 用户名
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// 邮箱
        /// </summary>
        string Email { get; }

        /// <summary>
        /// 是否通过认证
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// 当前 Token 字符串
        /// </summary>
        string Token { get; }

        /// <summary>
        /// UserToken 主键 Id
        /// </summary>
        long TokenId { get; }

        /// <summary>
        /// 当前登录平台（从 Token 中解析）
        /// </summary>
        int PlatformType { get; }

        /// <summary>
        /// 原始 ClaimsPrincipal
        /// </summary>
        ClaimsPrincipal RawClaimsPrincipal { get; }

        // 微信小程序相关（会员端）
        string CustomerId { get; }

        string CustomerPhone { get; }

        string WechatUnionID { get; }

        string CustomerTokenHash { get; }
    }
}
