using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Enums
{
    /// <summary>
    /// 系统平台
    /// 用于配置菜单权限等
    /// </summary>
    public enum PlatformType
    {
        /// <summary>
        /// 传0获取所有
        /// </summary>
        All = 0,

        /// <summary>
        /// 超管平台
        /// </summary>
        Admin = 1,

        /// <summary>
        /// PC业务系统
        /// </summary>
        Pc = 2,

        /// <summary>
        /// 微信小程序
        /// </summary>
        Mini = 3,

        /// <summary>
        /// POS机App
        /// </summary>
        Android = 4,
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
