using NexusStack.Infrastructure.Enums.OpenAppConfigs;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.OpenAppConfigs
{
    public class AppEventConfigDto
    {
        public long? Id { get; set; }
        public long AppId { get; set; }

        public required string Name { get; set; }

        public string? Method { get; set; }

        public string? EventCode { get; set; }

        public string? HookUrl { get; set; }

        public WebHookType Type { get; set; }

        public bool IsEnabled { get; set; }
    }
}
