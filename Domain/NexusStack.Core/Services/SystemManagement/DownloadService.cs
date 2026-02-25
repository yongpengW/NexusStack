using AutoMapper;
using LinqKit;
using NexusStack.Core.Dtos.DownloadCenter;
using NexusStack.Core.Entities.SystemManagement;
using NexusStack.Core.Services.Interfaces;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository;
using NexusStack.Excel.ExportStream;
using NexusStack.Infrastructure;
using NexusStack.Infrastructure.Exceptions;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace NexusStack.Core.Services.SystemManagement
{
    public class DownloadService(MainContext dbContext, IMapper mapper,
    Lazy<IUserService> userService,
    Lazy<ICurrentUser> currentUser
    ) : ServiceBase<DownloadItem>(dbContext, mapper), IDownloadService, IScopedDependency
    {
        private List<ExportExcelTypeMapDto>? _exportTypeMap;
        private List<ExportExcelTypeMapDto> ExportTypeMap => _exportTypeMap ??= InitExportTypeMap();

        private List<ExportExcelTypeMapDto> InitExportTypeMap()
        {
            //lazy load export type map
            return
            [
                // To Do
            ];
        }


        private ExportExcelTypeMapDto GetExportTypeMap(string typeName)
        {
            var exportType = ExportTypeMap.FirstOrDefault(x => x.TypeName == typeName)
                ?? throw new ArgumentException($"未找到导出类型 {typeName}");

            return exportType;
        }

        public async Task<byte[]> ExportExcelAsync(string typeName, string queryData, string? password = null)
        {
            var exportType = GetExportTypeMap(typeName);

            var queryModel = JsonSerializer.Deserialize(queryData, exportType.QueryModel)
                ?? throw new ArgumentException($"无法反序列化查询数据为 {exportType.QueryModel.Name}");

            var method = exportType.Method
                ?? throw new ArgumentException($"未找到导出方法 {exportType.Method}");

            var dataByte = await method(queryModel);

            if (exportType.Encrypt && !string.IsNullOrEmpty(password))
            {
                dataByte = ExcelEncryptHelper.EncryptExcel(dataByte, password);
            }

            return dataByte;
        }
    }
}
