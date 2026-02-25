using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos
{
    /// <summary>
    /// 开放API请求参数基类
    /// </summary>
    public class OpenApiBaseRequestDto
    {
        public string method { get; set; } = string.Empty;
        public string appKey { get; set; } = string.Empty;
        public string session { get; set; } = string.Empty;
        public string v { get; set; } = "1.0";
        public string signMethod { get; set; } = "md5";
        public string sign { get; set; } = string.Empty;
        public string timestamp { get; set; } = string.Empty;
    }
}
