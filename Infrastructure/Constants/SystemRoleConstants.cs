using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Constants
{
    /// <summary>
    /// 系统基础角色配置
    /// </summary>
    public class SystemRoleConstants
    {
        /// <summary>
        /// 超级管理员
        /// </summary>
        public const string Root = "root";
        /// <summary>
        /// 默认角色 初始注册用户使用的角色 即普通员工
        /// </summary>
        public const string Default = "staff";

        /// <summary>
        /// 系统默认角色Id 默认角色的ID
        /// </summary>
        public const long DefaultRoleId = 1;

        /// <summary>
        /// 系统默认区域Id
        /// </summary>
        public const long DefaultRegionId = 0;

        /// <summary>
        /// 系统默认门店Id
        /// </summary>
        public const long DefaultShopId = 0;
    }
}
