using NexusStack.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Swagger
{
    public class SwaggerOptions : IOptions
    {
        public string SectionName => "Swagger";

        /// <summary>
        /// 启用
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// 生成 Swagger 文档
        /// </summary>
        public bool SwaggerGen { get; set; }

        /// <summary>
        /// 启用 Swagger UI
        /// </summary>
        public bool SwaggerUI { get; set; }

        public List<SwaggerEndpoint> Endpoints { get; set; }
    }

    public class SwaggerEndpoint
    {
        public string Url { get; set; }

        public string Name { get; set; }
    }
}
