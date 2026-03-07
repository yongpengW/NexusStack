using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace NexusStack.RabbitMQ
{
    public class Connection : IConnection, IDisposable
    {
        private readonly RabbitOptions options;
        private readonly SemaphoreSlim connectionLock = new(1, 1);
        private global::RabbitMQ.Client.IConnection? cachedConnection;

        public Connection(IOptions<RabbitOptions> options)
        {
            this.options = options.Value;
        }

        public async Task<global::RabbitMQ.Client.IConnection> CreateConnectionAsync()
        {
            if (this.cachedConnection?.IsOpen == true)
            {
                return this.cachedConnection;
            }

            await this.connectionLock.WaitAsync();
            try
            {
                if (this.cachedConnection?.IsOpen == true)
                {
                    return this.cachedConnection;
                }

                var factory = new ConnectionFactory
                {
                    HostName = this.options.HostName,
                    Port = this.options.Port,
                    UserName = this.options.Username,
                    Password = this.options.Password,
                    VirtualHost = this.options.VirtualHost,

                    ConsumerDispatchConcurrency = this.options.ConsumerDispatchConcurrency,
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                this.cachedConnection = await factory.CreateConnectionAsync(this.options.ClientName);
                return this.cachedConnection;
            }
            finally
            {
                this.connectionLock.Release();
            }
        }

        public async Task<IChannel> CreateChannelAsync()
        {
            var connection = await CreateConnectionAsync();

            ushort? consumerDispatchConcurrency = this.options.ConsumerDispatchConcurrency == 0
                ? null
                : this.options.ConsumerDispatchConcurrency;

            var createChannelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true,
                outstandingPublisherConfirmationsRateLimiter: null,
                consumerDispatchConcurrency: consumerDispatchConcurrency);

            return await connection.CreateChannelAsync(createChannelOptions);
        }

        public void Dispose()
        {
            try
            {
                if (this.cachedConnection is not null)
                {
                    this.cachedConnection.CloseAsync().GetAwaiter().GetResult();
                    this.cachedConnection.Dispose();
                    this.cachedConnection = null;
                }
            }
            finally
            {
                this.connectionLock.Dispose();
            }
        }
    }
}
