using NexusStack.EFCore.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.SystemManagement
{
    /// <summary>
    /// 接口资源
    /// </summary>
    public class ApiResource : AuditedEntity
    {
        /// <summary>
        /// 接口名称
        /// </summary>
        [MaxLength(256)]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 接口标识
        /// </summary>
        [MaxLength(256)]
        public string? Code { get; set; }

        /// <summary>
        /// 所有接口按 Controller 分组，分组名称为 Controller 注释
        /// </summary>
        [MaxLength(256)]
        public string? GroupName { get; set; }

        /// <summary>
        /// 路由匹配模式
        /// </summary>
        [MaxLength(256)]
        public string? RoutePattern { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        [MaxLength(256)]
        public string? NameSpace { get; set; }

        /// <summary>
        /// 控制器名称
        /// </summary>
        [MaxLength(256)]
        public string? ControllerName { get; set; }

        /// <summary>
        /// 操作名称
        /// </summary>
        [MaxLength(256)]
        public string? ActionName { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        [MaxLength(256)]
        public string? RequestMethod { get; set; }
    }
}
