using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.RabbitMQ
{
    public class Connection : IConnection
    {
        private readonly RabbitOptions options;

        public Connection(IOptions<RabbitOptions> options)
        {
            this.options = options.Value;
        }

        public async Task<global::RabbitMQ.Client.IConnection> CreateConnectionAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = this.options.HostName,
                Port = this.options.Port,
                UserName = this.options.Username,
                Password = this.options.Password,
                VirtualHost = this.options.VirtualHost,

                ConsumerDispatchConcurrency = this.options.ConsumerDispatchConcurrency,
            };

            return await factory.CreateConnectionAsync(this.options.ClientName);
        }

        public async Task<IChannel> CreateChannelAsync()
        {
            var connection = await CreateConnectionAsync();
            return await connection.CreateChannelAsync();
        }
    }
}
