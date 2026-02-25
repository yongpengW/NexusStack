using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Messages
{
    public class SmsConfigurationDto
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string SignName { get; set; }
        public string TemplateCode { get; set; }
    }
}
