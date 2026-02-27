using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.OpenAppConfigs
{
    public class AppConfigViewDto
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
        public List<AppNotificationConfigDto>? ApiNotificationConfig { get; set; }
        public List<AppEventConfigDto>? ApiEventConfig { get; set; }
        public List<AppWebhookConfigDto>? WebhookConfig { get; set; }
    }
}
