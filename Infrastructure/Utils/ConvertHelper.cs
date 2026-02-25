using NexusStack.Infrastructure.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace NexusStack.Infrastructure.Utils
{
    public static class ConvertHelper
    {
        public static T To<T>(object value)
        {
            return (T)To(value, typeof(T));
        }

        public static object To(object value, Type destinationType)
        {
            if (value != null)
            {
                var sourceType = value.GetType();

                var destinationConverter = GetCustomTypeConverter(destinationType);
                var sourceConverter = GetCustomTypeConverter(sourceType);

                if (destinationConverter != null && destinationConverter.CanConvertFrom(value.GetType()))
                    return destinationConverter.ConvertFrom(value);

                if (sourceConverter != null && sourceConverter.CanConvertTo(destinationType))
                    return sourceConverter.ConvertTo(value, destinationType);

                if (destinationType.IsEnum && value is int)
                    return Enum.ToObject(destinationType, (int)value);

                if (!destinationType.IsInstanceOfType(value))
                    return Convert.ChangeType(value, destinationType);
            }
            return value;
        }

        public static TypeConverter GetCustomTypeConverter(Type type)
        {
            if (type == typeof(List<int>))
                return new GenericListTypeConverter<int>();

            if (type == typeof(List<decimal>))
                return new GenericListTypeConverter<decimal>();

            if (type == typeof(List<string>))
                return new GenericListTypeConverter<string>();

            return TypeDescriptor.GetConverter(type);
        }


        public static Dictionary<string, string> ObjectToMap(object obj)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

            Type t = obj.GetType(); // 获取对象对应的类， 对应的类型

            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance); // 获取当前type公共属性

            foreach (PropertyInfo p in pi)
            {
                MethodInfo m = p.GetGetMethod();

                if (m != null && m.IsPublic)
                {
                    // 进行判NULL处理
                    if (m.Invoke(obj, new object[] { }) != null)
                    {
                        map.Add(p.Name, m.Invoke(obj, new object[] { })
                                         .ToString()); // 向字典添加元素
                    }
                }
            }
            return map;
        }
    }
}
