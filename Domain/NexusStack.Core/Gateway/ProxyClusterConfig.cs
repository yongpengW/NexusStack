using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Gateway
{
    public class ProxyClusterConfig
    {
        public string ClusterId { get; set; } = string.Empty;
        public Dictionary<string, ProxyDestinationConfig> Destinations { get; set; } = new();
        public string? LoadBalancingPolicy { get; set; }
        public HealthCheckConfig? HealthCheck { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class ProxyDestinationConfig
    {
        public string Address { get; set; } = string.Empty;
        public string? Health { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class HealthCheckConfig
    {
        public bool Enabled { get; set; }
        public string? Interval { get; set; }
        public string? Timeout { get; set; }
        public string? Path { get; set; }
    }
}
