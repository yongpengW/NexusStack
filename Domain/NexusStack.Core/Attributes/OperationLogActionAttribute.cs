using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Attributes
{
    /// <summary>
    /// 操作日志特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class OperationLogActionAttribute : Attribute
    {
        /// <summary>
        /// 所属模块，如"用户管理"、"角色管理"
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// 操作描述模板，支持 {参数名} 占位符，如"创建用户 {userName}"
        /// </summary>
        public string MessageTemplate { get; set; }

        public OperationLogActionAttribute(string messageTemplate = "", string module = "")
        {
            MessageTemplate = messageTemplate;
            Module = module;
        }
    }
}
