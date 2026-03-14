using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NexusStack.Core.Dtos;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Excel.ExportStream;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Utils;
using NexusStack.Infrastructure.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.SystemManagement
{
    public class OperationLogService(MainContext dbContext, IMapper mapper) : ServiceBase<OperationLog>(dbContext, mapper), IOperationLogService, IScopedDependency
    {
        /// <summary>
        /// 记录操作日志
        /// </summary>
        /// <param name="code">操作菜单</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task LogAsync(string code, string content, string json, string ipAddress, string userAgent, LogType logType, string method, long userId = 0)
        {
            var entity = new OperationLog
            {
                IpAddress = ipAddress,
                OperationMenu = code ?? "",
                OperationContent = content.IsNotNullOrEmpty() ? content : json,
                UserAgent = userAgent,
                MenuCode = code ?? "",
                //Remark = json,
                LogType = logType,
                Method = method,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            await this.InsertAsync(entity);
        }


        public async Task SystemLogAsync(SystemLogDto log)
        {
            var entity = new OperationLog
            {
                IpAddress = "::1",
                OperationMenu = "",
                OperationContent = log.title ?? "",
                UserAgent = "",
                MenuCode = "",
                Remark = JsonConvert.SerializeObject(log.entity),
                LogType = log.logType,
                CreatedBy = log.userId,
                UpdatedBy = log.userId
            };

            await this.InsertAsync(entity);
        }

        /// <summary>
        /// 记录API请求操作日志
        /// </summary>
        /// <param name="code">操作菜单</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task RequestLogAsync(CreateOperationLogDto model, long userId)
        {
            var entity = new OperationLog
            {
                IpAddress = model.IpAddress,
                OperationMenu = model.OperationMenu,
                OperationContent = model.OperationContent ?? "",
                UserAgent = model.UserAgent,
                MenuCode = model.MenuCode,
                Remark = model.Remark,
                LogType = LogType.Request,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            await this.InsertAsync(entity);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task ExceptionLogAsync(CreateOperationLogDto model, long userId)
        {
            var entity = new OperationLog
            {
                IpAddress = model.IpAddress,
                OperationMenu = model.OperationMenu,
                OperationContent = model.OperationContent ?? "",
                UserAgent = model.UserAgent,
                MenuCode = model.MenuCode,
                Remark = model.Remark,
                LogType = model.LogType,
                ErrorTracker = model.ErrorTracker,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            await this.InsertAsync(entity);
        }

        public byte[] ExportLogAsync(List<OperationLogDto> logs)
        {
            var columnsMapping = new Dictionary<string, string>
                {
                    { "日志类型", "LogType" },
                    { "日志时间", "CreatedAt" },
                    { "操作人", "CreatedBy" },
                    { "操作内容", "OperationContent" },
                    { "操作菜单", "OperationMenu" },
                    { "浏览器类型", "UserAgent" },
                    { "请求参数", "RequestJson" },
                    { "IP地址", "IpAddress" }
                };
            return ExportExcelHelper.ExportToExcel(logs.OrderByDescending(x => x.CreatedBy), columnsMapping, "OperationLogs");
        }
    }
}
