using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Excel.ExportStream
{
    /// <summary>
    /// Excel 加密工具（基于 EPPlus）
    /// </summary>
    public static class ExcelEncryptHelper
    {
        /// <summary>
        /// 使用密码对 xlsx 字节流进行加密
        /// </summary>
        /// <param name="xlsxBytes">未加密的 xlsx 字节流</param>
        /// <param name="password">打开密码</param>
        /// <returns>加密后的 xlsx 字节流</returns>
        public static byte[] EncryptExcel(byte[] xlsxBytes, string password)
        {
            if (xlsxBytes == null || xlsxBytes.Length == 0) return xlsxBytes;
            if (string.IsNullOrWhiteSpace(password)) return xlsxBytes;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var inputStream = new MemoryStream(xlsxBytes);
            using var package = new ExcelPackage(inputStream);

            // 设置密码保护
            package.Encryption.Password = password.Trim();

            // 保存加密后的文件
            using var outputStream = new MemoryStream();
            package.SaveAs(outputStream);

            return outputStream.ToArray();
        }
    }
}
