using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Client
{
    public class RequestModel
    {
        public string Url { get; set; } = string.Empty;
        public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;
        public Dictionary<string, string> HttpContent { get; set; } = new Dictionary<string, string>();
        public Hashtable Header { get; set; } = new Hashtable();
    }

    public class PostRequest : RequestModel
    {
        public PostRequest()
        {
            HttpMethod = HttpMethod.Post;
            HttpContent = new Dictionary<string, string>();
            Header = new Hashtable
            {
                { "Content-Type", "application/json" },
                { "charset", "UTF-8" },
                { "Accept", "application/json" }
            };
        }
    }
}
