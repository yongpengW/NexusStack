using LoxSmoke.DocXml;
using NexusStack.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NexusStack.Infrastructure
{
    public class DocsHelper
    {
        private static List<string> Docs { get; set; } = [];

        private static void LoadDocs()
        {
            if (Docs.Count == 0)
            {
                // 基础目录
                var baseDir = AppContext.BaseDirectory ?? string.Empty;

                // 获取程序执行目录下所有 xml 文档
                Docs = [.. Directory.GetFiles(baseDir, "*.xml")];

                // 组合 Etouch 子目录（跨平台安全）
                var etouchDir = Path.Combine(baseDir, "Etouch");
                if (Directory.Exists(etouchDir))
                {
                    Docs.AddRange(Directory.GetFiles(etouchDir, "*.xml"));
                }
            }
        }

        public static string GetDocPath(string assemblyName)
        {
            LoadDocs();

            assemblyName = assemblyName.Replace(".dll", "").Replace(".xml", "");
            var docPath = Docs.FirstOrDefault(a => Path.GetFileNameWithoutExtension(a).ToLower() == assemblyName.ToLower());
            if (docPath.IsNullOrEmpty())
            {
                return string.Empty;
            }

            return docPath;
        }

        public static string GetDocPathByFileName(string fileName)
        {
            LoadDocs();
            var docPath = Docs.FirstOrDefault(a => Path.GetFileName(a).ToLower() == fileName.ToLower());
            if (docPath.IsNullOrEmpty())
            {
                return string.Empty;
            }
            return docPath;
        }

        public static string GetTypeComments(string assemblyName, Type type)
        {
            var docPath = GetDocPath(assemblyName);
            if (docPath.IsNullOrEmpty())
            {
                return string.Empty;
            }

            var reader = new DocXmlReader(docPath);
            return reader.GetTypeComments(type).Summary;
        }

        public static string GetMemberComments(string assemblyName, MemberInfo member)
        {
            var docPath = GetDocPath(assemblyName);
            if (docPath.IsNullOrEmpty() || member is null)
            {
                return string.Empty;
            }

            var reader = new DocXmlReader(docPath);
            return reader.GetMemberComments(member).Summary;
        }

        public static string GetMethodComments(string assemblyName, MethodInfo method)
        {
            var docPath = GetDocPath(assemblyName);
            if (docPath.IsNullOrEmpty())
            {
                return string.Empty;
            }

            var reader = new DocXmlReader(docPath);
            return reader.GetMethodComments(method).Summary;
        }
    }
}
