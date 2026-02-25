using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Gateway
{
    public class ProxyRouteConfig
    {
        public string RouteId { get; set; } = string.Empty;
        public string ClusterId { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public Dictionary<string, string>? Headers { get; set; }
        public string[]? Methods { get; set; }
        public int Order { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
