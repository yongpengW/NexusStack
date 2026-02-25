using NexusStack.Infrastructure.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.OpenApiConfigs
{
    public class OpenApiConfigDto : AuditedDtoBase
    {
        public required string AppKey { get; set; }

        public required string AppName { get; set; }

        public required string SecretKey { get; set; }

        public required string Sessionkey { get; set; }

        public string? AccessToken { get; set; }

        public long? AccessValidTime { get; set; }

        public bool IsEnabled { get; set; }

        public long? ShopId { get; set; }
    }
}
