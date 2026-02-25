using NexusStack.Infrastructure.Options;
using Snowflake.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.SnowFlake
{
    /// <summary>
    /// 分布式雪花Id生成器
    /// </summary>
    public class SnowFlake
    {
        /// <summary>
        /// 通过静态类只实例化一次IdWorker 否则生成的Id会有重复
        /// </summary>
        private static readonly Lazy<IdWorker> _instance = new(() =>
        {
            var commonOptions = App.Options<CommonOptions>();

            return new IdWorker(commonOptions.WorkerId, commonOptions.DatacenterId);
        });

        public static IdWorker Instance = _instance.Value;

        /// <summary>
        /// 销售订单Id生成器
        /// 销售订单种子为8
        /// </summary>
        private static readonly Lazy<IdWorker> _salesOrderInstance = new(() =>
        {
            var commonOptions = App.Options<CommonOptions>();
            return new IdWorker(commonOptions.WorkerId, 8);
        });

        public static IdWorker SalesOrderInstance = _salesOrderInstance.Value;

        /// <summary>
        /// 退货订单Id生成器
        /// 退货订单种子为6
        /// </summary>
        private static readonly Lazy<IdWorker> _returnOrderInstance = new(() =>
        {
            var commonOptions = App.Options<CommonOptions>();
            return new IdWorker(commonOptions.WorkerId, 6);
        });

        public static IdWorker ReturnOrderInstance = _returnOrderInstance.Value;
    }
}
