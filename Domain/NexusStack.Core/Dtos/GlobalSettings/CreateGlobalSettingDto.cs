using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.GlobalSettings
{
    public class CreateGlobalSettingDto
    {
        public long? Id { get; set; }

        public string AppId { get; set; }

        public string ConfigurationJson { get; set; }

        public string Key { get; set; }
    }
}
