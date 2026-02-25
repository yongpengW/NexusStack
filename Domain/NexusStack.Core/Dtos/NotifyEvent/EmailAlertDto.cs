using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.NotifyEvent
{
    public class EmailAlertDto
    {
        public EmailAlertDto()
        {
            toAddress = new List<string>();
        }
        public List<string> toAddress { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public List<EmailAttachmentDto>? Attachments { get; set; }
    }

    public class EmailAttachmentDto
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}
