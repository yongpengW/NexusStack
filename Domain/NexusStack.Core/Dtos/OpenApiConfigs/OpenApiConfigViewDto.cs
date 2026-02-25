using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.OpenApiConfigs
{
    public class OpenApiConfigViewDto
    {
        public long Id { get; set; }
        public string? AppName { get; set; }
        public string? AppKey { get; set; }
        public string? Sessionkey { get; set; }
        public string? SecretKey { get; set; }
        public long? AccessValidTime { get; set; }
        public bool IsEnabled { get; set; }
        public string? Remark { get; set; }
        public long? ShopId { get; set; }
    }

    public class OpenApiConfigDetailDto
    {
        public List<ApiNotificationConfigDto>? ApiNotificationConfig { get; set; }
        public List<ApiEventConfigDto>? ApiEventConfig { get; set; }
        public List<WebhookConfigDto>? WebhookConfig { get; set; }
    }
}
