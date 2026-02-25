using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Redis
{
    public class RedisOption
    {
        public string ConnectionString { get; set; }
        public bool UseKeyEventNotify { get; set; } = false;
    }
}
