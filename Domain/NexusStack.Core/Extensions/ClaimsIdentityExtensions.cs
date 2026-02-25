using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace NexusStack.Core.Extensions
{
    /// <summary>
    /// ClaimsIdentity扩展方法
    /// </summary>
    public static class ClaimsIdentityExtensions
    {
        public static void UpdateClaim(this ClaimsIdentity identity, string claimType, string claimValue)
        {
            var existingClaim = identity.FindFirst(claimType);
            if (existingClaim != null)
            {
                identity.RemoveClaim(existingClaim);
            }
            identity.AddClaim(new Claim(claimType, claimValue));
        }

        public static void UpdateClaimsFromDictionary(this ClaimsIdentity identity, Dictionary<string, string> claims)
        {
            foreach (var claim in claims)
            {
                identity.UpdateClaim(claim.Key, claim.Value);
            }
        }
    }
}
