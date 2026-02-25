using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NexusStack.Excel.Export
{
    public class SheetDto
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string SheetName { get; set; }

        /// <summary>
        /// 表数据
        /// </summary>
        public DataTable Data { get; set; }
    }

    public class DictSheetDto
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string SheetName { get; set; }

        /// <summary>
        /// 表数据
        /// </summary>
        public List<Dictionary<string, object>> Data { get; set; }
    }
}
