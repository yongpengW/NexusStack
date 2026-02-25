using NexusStack.Core.Entities.SystemManagement;
using NexusStack.EFCore.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Services.Interfaces
{
    public interface IDownloadService : IServiceBase<DownloadItem>
    {
        Task<byte[]> ExportExcelAsync(string typeName, string queryData, string? password = null);
    }
}
