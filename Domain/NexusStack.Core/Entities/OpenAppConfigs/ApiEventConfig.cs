using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums.OpenAppConfigs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.OpenAppConfigs
{
    public class ApiEventConfig : AuditedEntity
    {
        /// <summary>
        /// OpenApiConfig表主键Id
        /// </summary>
        public long AppId { get; set; }

        [MaxLength(256)]
        public required string Name { get; set; }

        [MaxLength(256)]
        public string? Method { get; set; }

        [MaxLength(256)]
        public string? EventCode { get; set; }

        [MaxLength(256)]
        public string? HookUrl { get; set; }

        public WebHookType Type { get; set; }

        public bool IsEnabled { get; set; }
    }
}
