using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NexusStack.Infrastructure.Enums.OpenAppConfigs
{
    /// <summary>
    /// 订阅类推送类型
    /// </summary>
    public enum WebHookType
    {
        DefaultValue = 0,

        /// <summary>
        /// 推送申报订单
        /// 表示此地址为ERP向第三方推送申报订单
        /// </summary>
        [Description("申报推送")]
        Declarer = 1,

        /// <summary>
        /// 推送退货订单
        /// 表示此地址为ERP向第三方推送退货订单
        /// </summary>
        [Description("退货推送")]
        Refund = 2,

        /// <summary>
        /// apos售后订单订阅接口
        /// 用于匹配查询当前售后单为哪个门店
        /// </summary>
        [Description("Apos售后订单订阅推送")]
        AposRefund = 3,

        /// <summary>
        /// 推送物流状态
        /// 表示此地址为ERP向第三方推送物流状态
        /// </summary>
        [Description("物流订阅推送")]
        Delivery = 4,

        /// <summary>
        /// apos自提柜存取订阅接口
        /// </summary>
        [Description("Apos自提柜存取订阅推送")]
        AposDelivery = 5,

        /// <summary>
        /// 清关查询地址
        /// </summary>
        [Description("清关查询")]
        StatusQuery = 6,

        /// <summary>
        /// 菜鸟清关取消
        /// ERP取消发货订单
        /// </summary>
        [Description("菜鸟清关取消")]
        CainiaoDeclarerCancel = 7,
    }
}
