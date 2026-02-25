using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.EFCore.Entities
{
    /// <summary>
    /// 数据表实体软删除标记
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// 是否删除
        /// </summary>
        bool IsDeleted { get; set; }
    }
}
