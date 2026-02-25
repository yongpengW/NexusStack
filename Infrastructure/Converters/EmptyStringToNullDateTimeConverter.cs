using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NexusStack.Infrastructure.Converters
{
    public class EmptyStringToNullDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                string value = reader.GetString();

                // 如果是空字符串，返回null
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                // 尝试解析日期
                if (DateTime.TryParse(value, out DateTime date))
                    return date;

                return null;
            }

            // 处理日期类型的直接输入
            if (reader.TokenType == JsonTokenType.Number)
            {
                return DateTime.UnixEpoch.AddMilliseconds(reader.GetInt64());
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Value);
        }
    }
}
