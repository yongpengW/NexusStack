using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.RabbitMQ
{
    public class RabbitOptions
    {
        /// <summary>
        /// 虚拟主机名称
        /// </summary>
        public string VirtualHost { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 主机名
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 客户端名称
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// 交换机名称
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// 消费者分发并发数
        /// </summary>
        public ushort ConsumerDispatchConcurrency { get; set; }

        /// <summary>
        /// 消费失败后最大重试次数（超过后进入死信队列）
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// 重试延迟毫秒数
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 5000;

        /// <summary>
        /// 是否启用消费幂等（去重）
        /// </summary>
        public bool EnableConsumerIdempotency { get; set; } = true;

        /// <summary>
        /// 消费幂等键过期小时数
        /// </summary>
        public int ConsumerIdempotencyExpireHours { get; set; } = 24;
    }
}
