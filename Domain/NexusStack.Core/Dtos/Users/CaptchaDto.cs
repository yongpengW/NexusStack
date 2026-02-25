using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Users
{
    /// <summary>
    /// 图形验证码
    /// </summary>
    public class CaptchaDto
    {
        /// <summary>
        /// 验证码(base64字符串)
        /// </summary>
        public string Captcha { get; set; }

        /// <summary>
        /// 验证码 Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 验证码过期时间
        /// </summary>
        public DateTime ExpireTime { get; set; }
    }
}
