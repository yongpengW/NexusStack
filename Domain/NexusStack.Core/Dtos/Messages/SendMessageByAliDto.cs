using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Messages
{
    public class SendMessageByAliDto
    {
        public long PhoneNumbers { get; set; }

        public string signNameJson { get; set; }

        public string templateCode { get; set; }

        public string templateParamJson { get; set; }

        public string Content { get; set; }
    }
}
