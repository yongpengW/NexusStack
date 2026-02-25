using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Enums.OpenAppConfigs
{
    public enum ApiNoticeType
    {
        /// <summary>
        /// 微信通知
        /// </summary>
        WeiXin,
        /// <summary>
        /// 企业微信通知
        /// </summary>
        QyWeiXin,
        /// <summary>
        /// 钉钉通知
        /// </summary>
        Dingtalk,
        /// <summary>
        /// 站内消息
        /// </summary>
        Message,
        /// <summary>
        /// 短信通知
        /// </summary>
        SMS,
        /// <summary>
        /// 邮件通知
        /// </summary>
        Email
    }
}
