using Microsoft.AspNetCore.Http;
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

        public const string Roles = "roles";

        public const string Email = "email";

        public const string ClientId = "clientId";

        public const string RegionId = "regionId";

        public const string Regions = "regions";

        public const string Shops = "shops";

        public const string ShopId = "shopId";

        public const string RoleId = "roleId";

        public const string Token = "token";

        public const string TokenId = "tokenId";

        public const string PlatFormType = "platFormType";

        public const string PlatForms = "platForms";

        public const string IsEnable = "isEnable";
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

        public long[] Roles => GetUserArryInfo(CoreClaimTypes.Roles);
        public string Email => this.FindClaimValue(CoreClaimTypes.Email);

        public long[] Shops => GetUserArryInfo(CoreClaimTypes.Shops);

        public long[] Regions => GetUserArryInfo(CoreClaimTypes.Regions);

        /// <summary>
        /// 是否通过认证
        /// </summary>
        public bool IsAuthenticated => this.httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        /// <summary>
        /// 用户状态 (是否启用)
        /// </summary>
        public bool IsEnable => this.FindClaimBoolValue(CoreClaimTypes.IsEnable);

        public string Token => this.FindClaimValue(CoreClaimTypes.Token);

        public long TokenId => this.FindClaimValue<long>(CoreClaimTypes.TokenId);

        //public long SystemId => this.FindClaimValue<long>(CoreClaimTypes.ClientId);

        //public long TenantId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int[] PlatForms => GetPlatFormInfo(CoreClaimTypes.PlatForms);

        public string CustomerId => this.FindClaimValue(CustomerClaimTypes.CustomerId);

        public string CustomerPhone => this.FindClaimValue(CustomerClaimTypes.CustomerPhone);

        public string WechatUnionID => this.FindClaimValue(CustomerClaimTypes.WechatUnionID);

        public string CustomerTokenHash => this.FindClaimValue(CustomerClaimTypes.CustomerTokenHash);

        public ClaimsPrincipal RawClaimsPrincipal => httpContextAccessor.HttpContext?.User;

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

        public virtual long[] GetUserArryInfo(string claimType)
        {
            var value = FindClaimValue(claimType);
            if (string.IsNullOrEmpty(value))
            {
                return [];
            }
            return value.Split(',').Select(x => long.Parse(x)).ToArray();
        }

        public virtual int[] GetPlatFormInfo(string claimType)
        {
            var value = FindClaimValue(claimType);
            if (string.IsNullOrEmpty(value))
            {
                return [];
            }
            return value.Split(',').Select(x => int.Parse(x)).ToArray();
        }

        public virtual bool FindClaimBoolValue(string claimType)
        {
            var value = FindClaimValue(claimType);
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            return value == "1" ? true : false;
        }
    }
}
