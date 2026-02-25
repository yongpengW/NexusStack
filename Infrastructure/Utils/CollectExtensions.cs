using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Text;

namespace NexusStack.Infrastructure.Utils
{
    /// <summary>
    /// 数据采集扩展
    /// </summary>
    public static class CollectExtensions
    {

        public static dynamic GetRedisKeys()
        {
            Dictionary<string, object> redisKeys = new Dictionary<string, object>();
            return false;
        }

        /// <summary>
        /// 将DataTable首行转换为ExpandoObject对象
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static dynamic ConvertExpandoObject(this DataTable dataTable)
        {
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;

            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return null;
            }

            var myRow = dataTable.Rows[0];
            foreach (DataColumn col in myRow.Table.Columns)
            {
                if (myRow[col] == DBNull.Value)
                {
                    expandoObject[col.ColumnName] = "";
                }
                else
                {
                    expandoObject[col.ColumnName] = myRow[col];
                }
            }

            return expandoObject;
        }


        /// <summary>
        /// 将指标查询的参数进行转换，使参数名不以@开头的，都变成@开头的参数
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Dictionary<string, object> RebuilderMetaDataQueryParemeter(this Dictionary<string, object> parameters)
        {
            Dictionary<string, object> sqlParameters = new Dictionary<string, object>();
            foreach (var key in parameters.Keys)
            {
                if (!key.StartsWith('@'))
                {
                    sqlParameters["@" + key] = parameters[key];
                }
                else
                {
                    sqlParameters[key] = parameters[key];
                }
            }
            return sqlParameters;
        }

        /// <summary>
        /// 检查通过录入模型获取的数据中是否有id，如果没有则新建一个id，如果没有记录则建一条
        /// </summary>
        /// <param name="dt"></param>
        public static void CheckIdColumnAndAddRow(this DataTable dt)
        {
            if (dt == null)
            {
                return;
            }
            if (!dt.Columns.Contains("id"))
            {
                dt.Columns.Add("id");
                if (dt.Rows.Count == 0)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }
                dt.Rows[0]["id"] = Guid.NewGuid().ToString();
            }
        }
    }
}
