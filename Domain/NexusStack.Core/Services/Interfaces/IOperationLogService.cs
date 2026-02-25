using NexusStack.Core.Dtos;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.EFCore.Repository;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Interfaces
{
    /// <summary>
    /// 操作日志服务接口
    /// </summary>
    public interface IOperationLogService : IServiceBase<OperationLog>
    {
        /// <summary>
        /// 记录操作日志
        /// </summary>
        /// <param name="code">菜单Code</param>
        /// <param name="content">操作内容</param>
        /// <param name="remark">操作参数</param>
        /// <param name="ipAddress"></param>
        /// <param name="userAgent"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task LogAsync(string code, string content, string remark, string ipAddress, string userAgent, LogType logType, string method, long userId = 0);

        /// <summary>
        /// 系统内日志
        /// </summary>
        /// <param name="title"></param>
        /// <param name="json"></param>
        /// <param name="logType"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task SystemLogAsync(SystemLogDto dto);

        /// <summary>
        /// 记录API请求操作日志
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task RequestLogAsync(CreateOperationLogDto model, long userId);

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task ExceptionLogAsync(CreateOperationLogDto model, long userId);

        byte[] ExportLogAsync(List<OperationLogDto> logs);
    }
}
