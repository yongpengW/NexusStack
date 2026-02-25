using NexusStack.EFCore.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.OpenAppConfig
{
    /// <summary>
    /// 平台开放API配置
    /// </summary>
    public class OpenApiConfig : AuditedEntity
    {
        /// <summary>
        /// 应用Key 每个应用唯一
        /// </summary>
        [MaxLength(64)]
        public required string AppKey { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        [MaxLength(256)]
        public required string AppName { get; set; }

        /// <summary>
        /// 应用绑定的门店ID
        /// </summary>
        public long? ShopId { get; set; }

        /// <summary>
        /// 应用安全密钥
        /// </summary>
        [MaxLength(256)]
        public required string SecretKey { get; set; }

        /// <summary>
        /// 应用会话密钥
        /// </summary>
        [MaxLength(256)]
        public required string Sessionkey { get; set; }

        /// <summary>
        /// 应用请求Token
        /// 后期可以使用这个token来使第三方接入CW POS的SSO。目前阶段不用
        /// AccessToken中可以直接包含用户的权限信息，后期使用时记得加大下字段MaxLength以防止数据过大存不进数据库
        /// </summary>
        [MaxLength(256)]
        public string? AccessToken { get; set; }

        /// <summary>
        /// 应用请求接口的有效时间 单位：分钟
        /// </summary>
        public long? AccessValidTime { get; set; }

        [DefaultValue(true)]
        public bool IsEnabled { get; set; }
    }
}
