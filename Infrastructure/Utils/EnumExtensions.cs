using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NexusStack.Infrastructure.Utils
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum val)
        {
            var field = val.GetType().GetField(val.ToString());
            var customAttrbute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return customAttrbute == null ? val.ToString() : ((DescriptionAttribute)customAttrbute).Description;
        }
    }
}
