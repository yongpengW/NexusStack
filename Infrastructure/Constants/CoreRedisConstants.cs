using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NexusStack.Infrastructure.Constants
{
    public class CoreRedisConstants
    {
        private static string currentAssembly = Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// 默认过期时间 7 天
        /// </summary>
        public static int DefaultExpireSeconds = 60 * 60 * 24 * 7;

        /// <summary>
        /// 图片验证码缓存 Key
        /// </summary>
        public static string TokenCaptcha = $"{currentAssembly}:Captcha:{{0}}";

        /// <summary>
        /// 用户Token缓存 Key
        /// </summary>
        public static string UserToken = $"{currentAssembly}:UserToken:{{0}}";

        /// <summary>
        /// 会员Token缓存 Key
        /// </summary>
        public static string CustomerToken = $"{currentAssembly}:CustomerToken:{{0}}";

        /// <summary>
        /// 定时任务 Cache Key
        /// </summary>
        public static string ScheduleTaskCache = $"ScheduleTask:{{0}}";

        /// <summary>
        /// 微信小程序配置缓存Key
        /// </summary>
        public static string WechatAppletAppIdCache = $"{currentAssembly}:WechatAppletAppId:{{0}}";
    }
}
