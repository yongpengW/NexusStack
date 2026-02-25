using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace NexusStack.Excel
{
    public class ExcelHelper
    {
        public static DataTable ToDataTable<T>(IEnumerable<T> collection)
        {
            var props = typeof(T).GetProperties();
            var dt = new DataTable();

            foreach (PropertyInfo pi in props)
            {
                // 当字段类型是Nullable<>时
                Type colType = pi.PropertyType;
                if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {

                    colType = colType.GetGenericArguments()[0];

                }
                dt.Columns.Add(new DataColumn(pi.Name, colType));
            }

            if (collection.Count() > 0)
            {
                for (int i = 0; i < collection.Count(); i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in props)
                    {
                        object obj = pi.GetValue(collection.ElementAt(i), null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    dt.LoadDataRow(array, true);
                }
            }
            return dt;
        }


        public static long GetTimeStamp(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMicroseconds;
        }

        public static DateTime ConvertTimeStampToDateTime(string timeStamp)
        {
            if (!string.IsNullOrWhiteSpace(timeStamp))
            {
                long timeStampValue = long.Parse(timeStamp);
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMicroseconds(timeStampValue).ToLocalTime();
            }
            return DateTime.Now;
        }
    }
}
