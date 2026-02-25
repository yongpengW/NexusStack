using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Middlewares
{
    /// <summary>
    /// 所有错误异常日志记录中间件(ApiAsyncExceptionFilter周期之外的异常)
    /// </summary>
    public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                //如果是身份验证失败（主要给系统开放的API用）的异常，则组装成统一格式返回结果
                if (ex is AuthenticationFailureException authEx)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var result = new RequestResultModel
                    {
                        Code = StatusCodes.Status401Unauthorized,
                        Message = ex.Message
                    };

                    await context.Response.WriteAsJsonAsync(result);
                }

                logger.LogError(ex, ex.Message);
            }
        }
    }
}
