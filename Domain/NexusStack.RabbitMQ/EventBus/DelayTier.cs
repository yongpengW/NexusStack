namespace NexusStack.RabbitMQ.EventBus
{
    /// <summary>
    /// 延迟消息档位（TTL + DLX 固定档位）
    /// </summary>
    public enum DelayTier
    {
        /// <summary>1 分钟</summary>
        Minute1 = 1,

        /// <summary>3 分钟</summary>
        Minute3 = 3,

        /// <summary>5 分钟</summary>
        Minute5 = 5,

        /// <summary>10 分钟</summary>
        Minute10 = 10,

        /// <summary>30 分钟</summary>
        Minute30 = 30,

        /// <summary>1 小时</summary>
        Hour1 = 60,

        /// <summary>2 小时</summary>
        Hour2 = 120
    }
}
