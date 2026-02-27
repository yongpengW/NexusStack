using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.OpenAppConfigs
{
    public class AppConfigCreateDto
    {
        public long Id { get; set; }
        public string? AppName { get; set; }
        public long? AccessValidTime { get; set; }
        public bool IsEnabled { get; set; }
        public string? Remark { get; set; }
        public long? ShopId { get; set; }
    }
}
