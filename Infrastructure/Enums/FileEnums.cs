using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NexusStack.Infrastructure.Enums
{
    /// <summary>
    /// 文件存储类型
    /// </summary>
    public enum FileStorageType
    {
        Local = 0,  //本地存储
        Aliyun = 1,
        Huawei = 2,
        Tencent = 3,
        Aws = 4,
        Oss = 5
    }

    /// <summary>
    /// 文件类型
    /// </summary>
    public enum FileType
    {
        Other = 9,
        Picture = 1,
        Document = 2,
        Video = 3,
        Audio = 4,
    }

    public enum FileState
    {
        Normal = 0,
        Transcoding = 1,
        Disabled = 2,
        Success = 3,
        Fail = 4,
        Expire = 5
    }

    public enum ExportState
    {
        [Description("导出成功")]
        Success = 1,
        [Description("导出失败")]
        Failure = 2,
        [Description("已过期")]
        Expire = 3,
        [Description("导出中")]
        InProgress = 4,
    }
}
