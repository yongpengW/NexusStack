using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NexusStack.Core.Attributes;
using NexusStack.Core.Services.SystemManagement;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using X.PagedList;

namespace NexusStack.Core.Filters
{
    /// <summary>
    /// 异步请求结果过滤器
    /// </summary>
    public class RequestAsyncResultFilter(IOperationLogService operationLogService, ICurrentUser currentUser) : IAsyncResultFilter
    {
        /// <summary>
        /// 在返回结果之前调用，用于统一返回数据格式
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (Activity.Current is not null)
            {
                context.HttpContext.Response.Headers.Append("X-TraceId", Activity.Current?.TraceId.ToString());
            }

            // 检查是否标记了SkipResponseWrapper特性，如果有则跳过响应包装
            if (ShouldSkipResponseWrapper(context))
            {
                await next();
                return;
            }

            if (context.Result is BadRequestObjectResult badRequestObjectResult)
            {
                var resultModel = new RequestResultModel
                {
                    Success = false,
                    Code = badRequestObjectResult.StatusCode ?? StatusCodes.Status400BadRequest,
                    Message = "请求参数验证错误",
                    Data = badRequestObjectResult.Value
                };

                context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Result = new RequestJsonResult(resultModel);
            }
            else if (context.Result is StatusCodeResult statusCodeResult)
            {
                var resultModel = new RequestResultModel
                {
                    Success = statusCodeResult.StatusCode == 200,
                    Code = statusCodeResult.StatusCode,
                    Message = statusCodeResult.StatusCode == 200 ? "Success" : "请求发生错误",
                    Data = statusCodeResult.StatusCode == 200
                };

                context.Result = new RequestJsonResult(resultModel);
            }
            else if (context.Result is ObjectResult result)
            {
                if (result.Value is null)
                {
                    var resultModel = new RequestResultModel
                    {
                        Success = false,
                        Code = result.StatusCode ?? context.HttpContext.Response.StatusCode,
                        Message = "未请求到数据"
                    };
                    context.Result = new RequestJsonResult(resultModel);
                }
                else if (result.Value is not RequestJsonResult)
                {
                    if (result.Value is IPagedList pagedList)
                    {
                        var resultModel = new RequestPagedResultModel
                        {
                            Success = true,
                            Message = "Success",
                            Data = result.Value,
                            Total = pagedList.TotalItemCount,
                            Page = pagedList.PageNumber,
                            TotalPage = pagedList.PageCount,
                            Limit = pagedList.PageSize,
                            Code = result.StatusCode ?? context.HttpContext.Response.StatusCode
                        };

                        context.Result = new RequestJsonResult(resultModel);
                    }
                    else
                    {
                        var code = result.StatusCode ?? context.HttpContext.Response.StatusCode;
                        var resultModel = new RequestResultModel
                        {
                            Success = code == 200,
                            Code = code,
                            Message = "Success",
                            Data = result.Value
                        };

                        context.Result = new RequestJsonResult(resultModel);
                    }
                }
            }

            await next();
        }

        /// <summary>
        /// 检查是否应该跳过响应包装
        /// </summary>
        /// <param name="context">结果执行上下文</param>
        /// <returns>如果应该跳过则返回true，否则返回false</returns>
        private bool ShouldSkipResponseWrapper(ResultExecutingContext context)
        {
            // 只有当ActionDescriptor存在且为ControllerActionDescriptor类型时才继续检查
            if (context.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
            {
                return false;
            }

            // 检查控制器或操作是否标记了SkipResponseWrapperAttribute
            var hasSkipAttribute = controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(SkipResponseWrapperAttribute), true).Any() ||
                                   controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(SkipResponseWrapperAttribute), true).Any();

            return hasSkipAttribute;
        }
    }
}
