using NexusStack.EFCore.Entities;
using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NexusStack.Core.Entities.Schedules
{
    /// <summary>
    /// 种子数据更新任务
    /// </summary>
    public class SeedDataTask : EntityBase
    {
        /// <summary>
        /// 最后一次的文件修改时间
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        [MaxLength(256)]
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 种子数据所属类的类名
        /// </summary>
        [MaxLength(256)]
        public string? Code { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnable { get; set; } = true;

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime ExecuteTime { get; set; }

        /// <summary>
        /// 任务执行状态
        /// </summary>
        public ExecuteStatus ExecuteStatus { get; set; }

        /// <summary>
        /// 配置文件地址
        /// </summary>
        public string? ConfigPath { get; set; }
    }
}
