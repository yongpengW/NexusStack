using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexusStack.Core.Services.OpenAppConfigs;
using NexusStack.Infrastructure.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace NexusStack.Core.Authentication
{
    /// <summary>
    /// 开放API请求身份验证处理器
    /// </summary>
    public class RequestAuthenticationHandler(
        IOptionsMonitor<RequestAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IOpenApiConfigService openApiConfigService
    ) : AuthenticationHandler<RequestAuthenticationSchemeOptions>(options, logger, encoder, clock)
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var httpContext = Context.Request.HttpContext;
            var param = new Dictionary<string, string>();

            var claims = new List<Claim>
            {
                new (ClaimTypes.Name, "open_api_access")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            //平台的开放接口只允许POST请求
            if (httpContext.Request.Method == HttpMethod.Post.Method
                && httpContext.Request.ContentType.ToLower().Contains("application/json"))
            {
                Request.EnableBuffering();

                Request.Body.Position = 0;
                using var streamReader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true);
                var requestBody = await streamReader.ReadToEndAsync();
                Request.Body.Position = 0;
                var jsonObject = JsonConvert.DeserializeObject<JObject>(requestBody);
                foreach (var property in jsonObject)
                {
                    param.Add(property.Key, property.Value.ToString());
                }
                if (!param.ContainsKey("sign") || !param.ContainsKey("timestamp") || !param.ContainsKey("appKey") || !param.ContainsKey("session"))
                {
                    throw new AuthenticationFailureException("缺少必要的签名参数[appKey,session,timestamp,sign]");
                }

                var app = await openApiConfigService.GetAsync(a => a.AppKey == param["appKey"] && a.IsEnabled);
                if (app == null)
                {
                    throw new AuthenticationFailureException("未授权应用, 无效的appKey");
                }

                if (app.Sessionkey.ToLower() != param["session"].ToLower())
                {
                    throw new AuthenticationFailureException("未授权应用, 无效的session");
                }

                var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var isTimestampValid = (time - long.Parse(param["timestamp"])) <= (app.AccessValidTime * 60 * 1000);
                if (!isTimestampValid)
                {
                    throw new AuthenticationFailureException("请求已过期，请注意API请求的有效时间");
                }

                // 校验签名
                var securityKey = app.SecretKey;
                var sign = param["sign"].ToString();
                param.Remove("sign");
                var encryptSign = EncryptionHelp.SignMD5Request(param, securityKey);
                if (encryptSign.ToLower() != sign.ToLower())
                {
                    throw new AuthenticationFailureException("签名验证失败");
                }
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                throw new AuthenticationFailureException("请求方式不正确，开放接口只允许POST请求");
            }
        }
    }
}
