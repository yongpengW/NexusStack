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

        string UserName { get; }

        /// <summary>
        /// 当前登录用户角色池
        /// </summary>
        long[] Roles { get; }

        string Email { get; }

        /// <summary>
        /// 当前登录用户所属店铺池
        /// </summary>
        long[] Shops { get; }

        long[] Regions { get; }

        /// <summary>
        /// 是否通过认证
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// 用户状态 是否启用
        /// </summary>
        bool IsEnable { get; }
        /// <summary>
        /// 当前 Token
        /// </summary>
        string Token { get; }

        /// <summary>
        /// UserToken Id
        /// </summary>
        long TokenId { get; }

        /// <summary>
        ///  所属系统Id 也可以是SSO分发的ClientId
        /// </summary>
        //long SystemId { get; }

        /// <summary>
        /// 所属租户Id
        /// </summary>
        //long TenantId { get; set; }

        /// <summary>
        /// 当前登录用户所属平台池
        /// </summary>
        int[] PlatForms { get; }

        ClaimsPrincipal RawClaimsPrincipal { get; }

        //微信小程序相关
        string CustomerId { get; }

        string CustomerPhone { get; }

        string WechatUnionID { get; }

        string CustomerTokenHash { get; }
    }
}
