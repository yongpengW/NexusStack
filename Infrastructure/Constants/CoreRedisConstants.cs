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
        /// 自定义Form表单缓存 Key
        /// </summary>
        public static string CollectFormCache = $"{currentAssembly}:CollectFormCache:{{0}}";

        public static string CurrentUserCache = $"{currentAssembly}:CurrentUser:{{0}}";
        public static string CurrentPosDeviceCache = $"{currentAssembly}:PosDevice:{{0}}";

        /// <summary>
        /// 商品库存缓存Key
        /// </summary>
        public static string ProductInventoryCache = $"Inventory:Store:{{0}}:Warehouse:{{1}}:MychemID:{{2}}";

        /// <summary>
        /// 微信小程序配置缓存Key
        /// </summary>
        public static string WechatAppletAppIdCache = $"{currentAssembly}:WechatAppletAppId:{{0}}";

        public static string PosOrderPaymentLock = $"{currentAssembly}:PosOrderPaymentLock:{{0}}";

        /// <summary>
        /// 郑州快递单号
        /// 当前单号
        /// </summary>
        public static string ZhengzhouLogisticsNoCache = $"Zhengzhou:LogisticsNo";
        /// <summary>
        /// 郑州快递单号
        /// 最大值
        /// </summary>
        public static string ZhengzhouLogisticsNoMaxCache = $"Zhengzhou:LogisticsNoMax";

        /// <summary>
        /// 提货码可用集合 Key
        /// 格式：PickupCode:Available:ShopId:Date
        /// 存储已回收且可复用的提货码
        /// </summary>
        public static string AvailablePickupCodeSet = $"{currentAssembly}:PickupCode:Available:{{0}}:{{1}}";

        /// <summary>
        /// 每日提货码最大序列号 Key
        /// 格式：PickupCode:MaxNumber:ShopId:Date
        /// 存储当天已生成的最大提货码序列号
        /// </summary>
        public static string DailyPickupCodeMaxNumber = $"{currentAssembly}:PickupCode:MaxNumber:{{0}}:{{1}}";
    }
}
