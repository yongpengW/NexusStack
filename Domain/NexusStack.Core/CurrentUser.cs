using Microsoft.AspNetCore.Http;
using NexusStack.Core.Dtos.Users;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace NexusStack.Core
{
    public static class CoreClaimTypes
    {
        public const string UserId = "userId";

        public const string UserName = "userName";

        public const string Email = "email";

        public const string ClientId = "clientId";

        public const string Token = "token";

        public const string TokenId = "tokenId";

        public const string PlatFormType = "platFormType";

        public const string PlatForms = "platForms";

        /// <summary>
        /// HttpContext.Items 中存放用户上下文的 Key（Roles、Regions 等，由认证 Handler 写入）
        /// </summary>
        public const string UserContextItemsKey = "NexusStack.UserContext";
    }

    public static class CustomerClaimTypes
    {
        public const string CustomerId = "customerId";
        public const string CustomerPhone = "customerPhone";
        public const string WechatUnionID = "wechatUnionID";
        public const string CustomerTokenHash = "customerTokenHash";
    }

    /// <summary>
    /// 当前登录用户
    /// </summary>
    public class CurrentUser : ICurrentUser, IScopedDependency
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 当前登录用户 Id
        /// </summary>
        public long UserId => this.FindClaimValue<long>(CoreClaimTypes.UserId);

        public string UserName => this.FindClaimValue(CoreClaimTypes.UserName);

        public string Email => this.FindClaimValue(CoreClaimTypes.Email);

        /// <summary>
        /// 是否通过认证
        /// </summary>
        public bool IsAuthenticated => this.httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public string Token => this.FindClaimValue(CoreClaimTypes.Token);

        public long TokenId => this.FindClaimValue<long>(CoreClaimTypes.TokenId);

        //public long SystemId => this.FindClaimValue<long>(CoreClaimTypes.ClientId);

        //public long TenantId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int PlatformType => this.FindClaimValue<int>(CoreClaimTypes.PlatFormType);

        public string CustomerId => this.FindClaimValue(CustomerClaimTypes.CustomerId);

        public string CustomerPhone => this.FindClaimValue(CustomerClaimTypes.CustomerPhone);

        public string WechatUnionID => this.FindClaimValue(CustomerClaimTypes.WechatUnionID);

        public string CustomerTokenHash => this.FindClaimValue(CustomerClaimTypes.CustomerTokenHash);

        public ClaimsPrincipal RawClaimsPrincipal => httpContextAccessor.HttpContext?.User;

        /// <summary>
        /// 当前平台下该用户拥有的角色 Id 列表（从 HttpContext.Items 中的用户上下文读取）
        /// </summary>
        public IReadOnlyList<long> RoleIds => GetUserContext()?.RoleIds ?? (IReadOnlyList<long>)Array.Empty<long>();

        /// <summary>
        /// 用户所属组织/地区 Id 列表（从 HttpContext.Items 中的用户上下文读取）
        /// </summary>
        public IReadOnlyList<long> RegionIds => GetUserContext()?.RegionIds ?? (IReadOnlyList<long>)Array.Empty<long>();

        private UserContextCacheDto GetUserContext()
        {
            if (httpContextAccessor.HttpContext?.Items.TryGetValue(CoreClaimTypes.UserContextItemsKey, out var value) == true && value is UserContextCacheDto ctx)
                return ctx;
            return null;
        }

        public virtual Claim FindClaim(string claimType)
        {
            return this.httpContextAccessor.HttpContext?.User?.FindFirst(claimType);
        }

        public virtual string FindClaimValue(string claimType)
        {
            try
            {
                return FindClaim(claimType)?.Value;
            }
            catch (Exception exc)
            {
                return string.Empty;
            }

        }

        public virtual T FindClaimValue<T>(string claimType) where T : struct
        {
            var value = FindClaimValue(claimType);
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }
            return value.To<T>();
        }

        // 其余复杂信息（角色、区域、门店等）不再从 Claims 读取，而是通过业务服务或权限缓存按需查询
    }
}
