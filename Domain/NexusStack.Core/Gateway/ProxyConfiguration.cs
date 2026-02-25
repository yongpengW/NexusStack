using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Gateway
{
    public class ProxyConfiguration
    {
        public List<ProxyRouteConfig> Routes { get; set; } = new();
        public List<ProxyClusterConfig> Clusters { get; set; } = new();
    }
}
