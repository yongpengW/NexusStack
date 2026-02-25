using NexusStack.Infrastructure.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace NexusStack.Infrastructure.Options
{
    public class JsonOptions
    {
        private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = {
                new JsonLongConverter()
            }
        };

        public static JsonSerializerOptions Default
        {
            get
            {
                return _jsonSerializerOptions;
            }
        }
    }
}
