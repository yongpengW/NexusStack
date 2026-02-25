using NexusStack.Infrastructure.Enums.OpenAppConfigs;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.OpenApiConfigs
{
    public class ApiNotificationConfigDto
    {
        public long? Id { get; set; }
        public long AppId { get; set; }

        public required string Name { get; set; }

        public ApiNoticeType Type { get; set; }

        public string? Phones { get; set; }

        public string? NoticeUrl { get; set; }

        public bool IsEnabled { get; set; }
    }
}
