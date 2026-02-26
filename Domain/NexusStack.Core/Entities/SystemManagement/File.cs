using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.SystemManagement
{
    /// <summary>
    /// 文件
    /// </summary>
    public class File : AuditedEntity
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        [MaxLength(256)]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 文件类型
        /// </summary>
        public FileType Type { get; set; }

        /// <summary>
        /// 文件大小(字节) 1024字节=1KB
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        [MaxLength(16)]
        [Required]
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// 文件 Url
        /// </summary>
        [MaxLength(1024)]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 文件存储路径
        /// </summary>
        [MaxLength(1024)]
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 文件存储方式
        /// </summary>
        public FileStorageType StorageType { get; set; }

        /// <summary>
        /// 文件MIME
        /// </summary>
        [MaxLength(128)]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// 文件 MD5 Hash 值
        /// </summary>
        [MaxLength(64)]
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// 原文件 Id, 用于存储缩略图、裁剪图、视频转码等文件的原始文件编号，如果为 0 表示该文件为原始文件
        /// </summary>
        public long OriginalId { get; set; }

        /// <summary>
        /// 文件状态
        /// </summary>
        public FileState State { get; set; }
    }
}
