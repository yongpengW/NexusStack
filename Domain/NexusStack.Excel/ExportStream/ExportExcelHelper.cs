using NexusStack.Infrastructure.Utils;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Excel.ExportStream
{
    public class ExportExcelHelper
    {
        /// <summary>
        /// 导出Excel文件流
        /// </summary>
        /// <param name="data">要导出的数据</param>
        /// <param name="columnsMapping">列名与数据字段的映射关系</param>
        /// <param name="sheetName">工作表名称</param>
        /// <param name="fileName">文件名称</param>
        /// <returns>Excel文件流</returns>
        public static byte[] ExportToExcel<T>(IEnumerable<T> data, Dictionary<string, string> columnsMapping, string sheetName)
        {
            // 创建Excel工作簿
            IWorkbook workbook = new XSSFWorkbook();

            // 创建工作表
            ISheet sheet = workbook.CreateSheet(sheetName);

            // 创建标题行
            IRow headerRow = sheet.CreateRow(0);
            int column = 0;
            foreach (var columnName in columnsMapping.Keys)
            {
                ICell cell = headerRow.CreateCell(column++);
                cell.SetCellValue(columnName);
            }

            // 填充数据
            int rowNumber = 1;
            foreach (T item in data)
            {
                IRow row = sheet.CreateRow(rowNumber++);
                Type type = item.GetType();
                column = 0;
                foreach (var columnName in columnsMapping.Keys)
                {
                    var propertyInfo = type.GetProperty(columnsMapping[columnName]);
                    if (propertyInfo != null)
                    {
                        ICell cell = row.CreateCell(column++);
                        object value = propertyInfo.GetValue(item, null);
                        if (value != null)
                        {
                            // 根据属性值类型设置单元格值
                            if (value is Enum enumValue)
                            {
                                // 添加对枚举的特殊处理
                                cell.SetCellValue(enumValue.GetDescription());
                            }
                            else if (value is int || value is double || value is decimal)
                            {
                                cell.SetCellValue(Convert.ToDouble(value));
                            }
                            else if (value is DateTime)
                            {
                                cell.SetCellValue((DateTime)value);
                            }
                            else
                            {
                                cell.SetCellValue(value.ToString());
                            }
                        }
                    }
                }
            }
            using var stream = new MemoryStream();
            workbook.Write(stream);
            return stream.ToArray();
        }
    }
}
