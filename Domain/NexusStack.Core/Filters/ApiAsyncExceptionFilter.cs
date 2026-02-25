using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NexusStack.Core.EventData;
using NexusStack.Core.Services.SystemManagement;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Exceptions;
using NexusStack.Infrastructure.Models;
using NexusStack.Infrastructure.Options;
using NexusStack.RabbitMQ.EventBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NexusStack.Core.Filters
{
    /// <summary>
    /// 错误异常处理过滤器（控制器构造函数、执行Action接口方法、执行ResultFilter结果过滤器）
    /// </summary>
    public class ApiAsyncExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ILogger<ApiAsyncExceptionFilter> logger;
        private readonly IOperationLogService operationLogService;
        private readonly ICurrentUser currentUser;
        private readonly IEventPublisher publisher;

        public ApiAsyncExceptionFilter(IOperationLogService operationLogService, IEventPublisher publisher, ICurrentUser currentUser, ILogger<ApiAsyncExceptionFilter> logger)
        {
            this.logger = logger;
            this.operationLogService = operationLogService;
            this.publisher = publisher;
            this.currentUser = currentUser;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception;

            //设置错误返回结果
            var resultModel = new RequestResultModel();
            if (exception is ErrorCodeException errorCodeException)
            {
                resultModel.Code = errorCodeException.ErrorCode;
            }
            else if (exception is ForbiddenException forbiddenException)
            {
                resultModel.Code = (int)HttpStatusCode.Forbidden;
            }
            else
            {
                resultModel.Code = (int)HttpStatusCode.InternalServerError;
            }

            // 获取主异常消息
            resultModel.Message = exception.Message;

            // 获取所有内部异常消息
            var innerExceptionMessages = GetAllInnerExceptionMessages(exception);
            if (!string.IsNullOrEmpty(innerExceptionMessages))
            {
                resultModel.Message += $" Inner Exceptions: {innerExceptionMessages}";
            }

            // 读取配置文件中是否配置了显示堆栈信息
            if (App.Options<CommonOptions>().ShowStackTrace)
            {
                resultModel.Data = exception.StackTrace;
            }

            context.Result = new RequestJsonResult(resultModel);

            //用来指示错误异常已处理
            context.ExceptionHandled = true;

            logger.LogError(exception, exception.Message);

            #region 记录日志

            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            //获取API的描述
            var commentsInfo = DocsHelper.GetMethodComments(actionDescriptor.ControllerTypeInfo.Assembly.GetName().Name, actionDescriptor.MethodInfo);

            var menuCode = "";
            if (context.HttpContext.Request.Headers.ContainsKey("Menu-Code"))
            {
                menuCode = context.HttpContext.Request.Headers["Menu-Code"];
            }
            //获取请求的接口
            var apiPath = context.HttpContext.Request.Path;
            //获取请求的User-Agent
            var userAgent = context.HttpContext.Request.Headers["User-Agent"];
            //获取请求ip地址
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            //获取请求参数
            var parameters = new Dictionary<string, string>();
            // 判断取值方式
            if (context.HttpContext.Request.Method == "POST" || context.HttpContext.Request.Method == "PUT")
            {
                if (context.HttpContext.Request.ContentType != null)
                {
                    if (context.HttpContext.Request.ContentType.Contains("application/x-www-form-urlencoded")
                        || context.HttpContext.Request.ContentType.Contains("multipart/form-data"))
                    {
                        try
                        {
                            // 读取表单数据
                            var form = await context.HttpContext.Request.ReadFormAsync();
                            foreach (var formItem in form)
                            {
                                parameters[formItem.Key] = formItem.Value.FirstOrDefault();
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("Error reading form data", ex);
                        }
                    }
                    else if (context.HttpContext.Request.ContentType.Contains("application/json") &&
                         context.HttpContext.Request.Body != null)
                    {
                        try
                        {
                            // 将 Request.Body 复制到一个 MemoryStream 中，以便多次读取
                            context.HttpContext.Request.EnableBuffering();

                            // 读取 JSON 数据
                            using var reader = new StreamReader(context.HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true);
                            var jsonString = await reader.ReadToEndAsync();

                            // 解析 JSON 数据
                            var jsonParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
                            if (jsonParams != null)
                            {
                                foreach (var param in jsonParams)
                                {
                                    parameters[param.Key] = param.Value;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException("Error reading JSON data", ex);
                        }
                    }
                }
            }

            // 读取查询字符串参数
            foreach (var queryItem in context.HttpContext.Request.Query)
            {
                parameters[queryItem.Key] = queryItem.Value.FirstOrDefault();
            }

            // 读取路径参数
            var pathParameters = context.RouteData.Values.Where(w => w.Key == "id" || w.Key == "platformType");
            foreach (var parameter in pathParameters)
            {
                parameters[parameter.Key] = parameter.Value?.ToString();
            }

            // 将所有参数转换为JSON字符串
            var jsonParameters = JsonConvert.SerializeObject(parameters, Formatting.None);
            #endregion

            var jsonPara = JsonConvert.SerializeObject(resultModel.Message);
            //var model = new CreateOperationLogDto()
            //{
            //    IpAddress = ipAddress,
            //    OperationMenu = apiPath,
            //    OperationContent = commentsInfo,
            //    UserAgent = userAgent,
            //    MenuCode = menuCode,
            //    Remark = jsonParameters,
            //    LogType = LogType.Error,
            //    ErrorTracker = jsonPara,
            //};
            //await operationLogService.ExceptionLogAsync(model, currentUser.UserId);

            var method = context.HttpContext.Request.Method;

            var pushData = new OperationLogEventData();
            pushData.Code = apiPath;
            pushData.Content = jsonParameters;
            pushData.Json = jsonPara;
            pushData.UserId = currentUser.UserId;
            pushData.IpAddress = ipAddress;
            pushData.UserAgent = context.HttpContext.Request.Headers.UserAgent;
            pushData.Method = method ?? string.Empty;
            pushData.LogType = LogType.Error;

            publisher.Publish(pushData);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 递归获取所有内部异常的消息
        /// </summary>
        /// <param name="exception">异常对象</param>
        /// <returns>格式化的内部异常消息字符串</returns>
        private string GetAllInnerExceptionMessages(Exception exception)
        {
            if (exception.InnerException == null)
                return string.Empty;

            var messages = new StringBuilder();
            var currentException = exception.InnerException;
            var level = 1;

            while (currentException != null)
            {
                messages.Append($"[Level {level}] {currentException.Message}");

                if (currentException.InnerException != null)
                    messages.Append(" -> ");

                currentException = currentException.InnerException;
                level++;
            }

            return messages.ToString();
        }
    }
}
