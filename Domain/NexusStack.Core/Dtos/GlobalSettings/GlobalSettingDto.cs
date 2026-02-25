using NexusStack.Infrastructure.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.GlobalSettings
{
    public class GlobalSettingDto : AuditedDtoBase
    {
        public string AppId { get; set; }

        public string ConfigurationJson { get; set; }

        public string Key { get; set; }
    }
}
