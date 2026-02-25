using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.FileStroage
{
    public class AliyunOSSOption
    {
        /// <summary>
        /// 下载中心
        /// </summary>
        public AliyunOSSSetting DownloadCenter { get; set; } = new AliyunOSSSetting();

        /// <summary>
        /// 配置中心
        /// </summary>
        public AliyunOSSSetting ConfigurationCenter { get; set; } = new AliyunOSSSetting();

        /// <summary>
        /// 上传中心
        /// </summary>
        public AliyunOSSSetting UploadCenter { get; set; } = new AliyunOSSSetting();
    }
    public class AliyunOSSSetting
    {
        public string Endpoint { get; set; }
        public string AccessKeyId { get; set; }
        public string AccessKeySecret { get; set; }
        public string BucketName { get; set; }
        public int ExpirationDay { get; set; }
        public bool IsRemote { get; set; }
    }
}
