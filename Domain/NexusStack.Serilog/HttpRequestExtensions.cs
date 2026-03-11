using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace NexusStack.Serilog
{
    /// <summary>
    /// Http请求扩展
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// 获取当前请求的IP地址
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetRemoteIpAddress(this HttpRequest request)
        {
            if (request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var firstIp = forwardedFor
                    .ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(firstIp))
                {
                    return firstIp;
                }
            }

            var remoteIP = request.HttpContext.Connection.RemoteIpAddress;
            if (remoteIP == null)
            {
                return string.Empty;
            }

            if (remoteIP.IsIPv4MappedToIPv6)
            {
                remoteIP = remoteIP.MapToIPv4();
            }

            return remoteIP.ToString();
        }
    }
}
