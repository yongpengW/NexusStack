using NexusStack.Core.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace NexusStack.Core.Services.SystemManagement
{
    public class EnumService
    {
        public List<EnumOptionDto> EnumToList<T>()
        {
            List<EnumOptionDto> list = new List<EnumOptionDto>();

            foreach (FieldInfo myEnum in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                DescriptionAttribute[] arrDesc = (DescriptionAttribute[])myEnum.GetCustomAttributes(typeof(DescriptionAttribute), false);
                list.Add(new EnumOptionDto()
                {
                    value = (int)myEnum.GetValue(null),
                    label = arrDesc[0].Description
                });
            }

            return list;
        }
        public T? GetEnumValueByDescription<T>(string description) where T : struct
        {
            if (string.IsNullOrEmpty(description))
            {
                return null;
                //throw new ArgumentException("Description cannot be null or empty.");
            }

            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description.Equals(description, StringComparison.OrdinalIgnoreCase))
                    {
                        return (T)field.GetValue(null);
                    }
                }
            }

            return null;
        }
    }
}
