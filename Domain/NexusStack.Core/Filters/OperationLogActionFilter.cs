using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using NexusStack.Core.Attributes;
using NexusStack.Core.EventData;
using NexusStack.Core.Services.SystemManagement;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Enums;
using NexusStack.RabbitMQ.EventBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NexusStack.Serilog;

namespace NexusStack.Core.Filters
{
    /// <summary>
    /// 操作日志记录过滤器
    /// </summary>
    public class OperationLogActionFilter(IOperationLogService operationLogService, IEventPublisher publisher, ICurrentUser currentUser) : IAsyncActionFilter
    {
        /// <summary>
        /// 执行时机可通过代码中的的位置（await next();）来分辨
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="NotImplementedException"></exception>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 检查是否标记了 NoLogging 特性，如果有则跳过日志记录
            if (ShouldSkipLogging(context))
            {
                await next();
                return;
            }

            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            //var menuCode = context.HttpContext.Request.Headers["Menu-Code"].ToString();

            //var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            // 获取 Action 注释
            //var commentsInfo = DocsHelper.GetMethodComments(actionDescriptor.ControllerTypeInfo.Assembly.GetName().Name, actionDescriptor.MethodInfo);

            if (actionDescriptor != null)
            {
                //前端传递的接口参数
                var json = JsonConvert.SerializeObject(context.ActionArguments);

                //系统刚上线阶段 所有的接口都记录日志
                //OperationLogActionAttribute 标记自定义操作日志内容（可加入参数）
                var logAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<OperationLogActionAttribute>();

                var method = context.HttpContext.Request.Method;

                var logMessage = string.Empty;
                if (logAttribute != null)
                {
                    logMessage = logAttribute.MessageTemplate;
                    if (!string.IsNullOrEmpty(logMessage))
                    {
                        CreateOperationLogContent(json, ref logMessage);
                    }
                }

                var apiPath = context.HttpContext.Request.Path;
                //var userAgent = context.HttpContext.Request.Headers["User-Agent"];

                //修改为MQ记录操作日志
                var pushData = new OperationLogEventData();
                pushData.Code = apiPath;
                pushData.Content = logMessage;
                pushData.Json = json;
                pushData.UserId = currentUser.UserId;
                pushData.IpAddress = context.HttpContext.Request.GetRemoteIpAddress();
                pushData.UserAgent = context.HttpContext.Request.Headers.UserAgent!;
                pushData.Method = method ?? string.Empty;
                pushData.LogType = LogType.Request;

                publisher.Publish(pushData);

            }

            await next();
        }

        /// <summary>
        /// 根据OperationLogActionAttribute和参数生成操作日志的内容
        /// </summary>
        /// <param name="json"></param>
        /// <param name="logMessage"></param>
        private void CreateOperationLogContent(string json, ref string logMessage)
        {
            JObject jObject = JObject.Parse(json);

            // 正则提取参数
            Regex regex = new Regex(@"\{([\w\.]+)\}");
            MatchCollection matches = regex.Matches(logMessage);

            // 替换
            foreach (Match match in matches)
            {
                Console.WriteLine(match.Groups[1].Value);
                var key = match.Groups[1].Value;
                logMessage = logMessage.Replace($"{{{key}}}", jObject.SelectToken(key).Value<string>());
            }
        }
        /// <summary>
        /// 检查是否应该跳过日志记录
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool ShouldSkipLogging(ActionExecutingContext context)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor != null)
            {
                // 检查方法级别的特性
                var methodInfo = actionDescriptor.MethodInfo;
                if (methodInfo.GetCustomAttribute<NoLoggingAttribute>() != null)
                {
                    return true;
                }
                // 检查类级别的特性
                var classType = actionDescriptor.ControllerTypeInfo;
                if (classType.GetCustomAttribute<NoLoggingAttribute>() != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
