using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Enums
{
    /// <summary>
    /// 系统平台（支持按位组合，用于配置菜单/角色/Token 等）
    /// </summary>
    [Flags]
    public enum PlatformType
    {
        /// <summary>
        /// 表示所有平台（特殊值，一般用于查询过滤；不要与具体平台组合存库）
        /// </summary>
        All = 0,

        /// <summary>
        /// 超管平台
        /// </summary>
        Admin = 1 << 0,   // 1 (2^0)

        /// <summary>
        /// PC业务系统
        /// </summary>
        Pc = 1 << 1,      // 2 (2^1)

        /// <summary>
        /// 微信小程序
        /// </summary>
        Mini = 1 << 2,    // 4 (2^2)

        /// <summary>
        /// POS机App
        /// </summary>
        Android = 1 << 3, // 8 (2^3)
    }

    /// <summary>
    /// 登陆方式
    /// </summary>
    public enum LoginMethodType
    {
        /// <summary>
        /// 账号密码
        /// </summary>
        AccountPassword = 0,
        /// <summary>
        /// 短信验证码
        /// </summary>
        SMS = 1,
        /// <summary>
        /// 微信小程序
        /// </summary>
        WXApplet = 2,
        /// <summary>
        /// 扫描登录
        /// </summary>
        ScanCode = 3
    }

    public enum UserType
    {
        /// <summary>
        /// 全部
        /// </summary>
        All = 0,
        /// <summary>
        /// 游客
        /// </summary>
        Visitor = 1,
        /// <summary>
        /// 工作人员
        /// </summary>
        Staff = 2
    }

    /// <summary>
    /// 系统中的服务类型
    /// </summary>
    public enum CoreServiceType
    {
        /// <summary>
        /// Web服务
        /// </summary>
        WebService = 0,
        /// <summary>
        /// MQ服务
        /// </summary>
        MQService = 1,
        /// <summary>
        /// 计划任务服务
        /// </summary>
        PlanTaskService = 2,
        /// <summary>
        /// Gateway
        /// </summary>
        Gateway = 3
    }
}
