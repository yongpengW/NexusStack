using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Utils
{
    public static class FileHelper
    {
        public static string SaveFile(IFormFile file, string directoryPath, string fileName)
        {
            Stream stream = file.OpenReadStream();
            string tempDir = $"Upload/temp/admin/{DateTime.Now.ToString("yyyyMMddHHmmssfff")}/";
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            string filePath = Path.Combine(directoryPath, fileName);

            if (!File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    long length = file.Length;
                    int i = 1;
                    if (length > 1024)
                    {
                        byte[] buffer = null;
                        while (i * 1024 < length)
                        {
                            buffer = new byte[1024];
                            stream.Read(buffer, 0, 1024);//防止读取文件过大,1KB循环读取,fs.write之后，再次读取的是新字节内容
                            fs.Write(buffer, 0, buffer.Length);
                            i++;
                        }
                        buffer = new byte[1024];
                        stream.Read(buffer, 0, (int)length - ((i - 1) * 1024));
                        fs.Write(buffer, 0, (int)length - ((i - 1) * 1024));
                    }
                    else
                    {
                        byte[] buffer = new byte[1024];
                        stream.Read(buffer, 0, buffer.Length);
                        fs.Write(buffer, 0, buffer.Length);
                    }
                    fs.Flush();
                    fs.Close();
                }
            }

            return filePath;
        }
    }
}
