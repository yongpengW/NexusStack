using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Dtos
{
    public abstract class AuditedDtoBase : AuditedDtoBase<long>
    {
    }

    public abstract class AuditedDtoBase<TKey> : DtoBase<TKey>
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 创建人id
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// 创建人姓名
        /// </summary>
        public string CreatedUser { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// 修改人id
        /// </summary>
        public long UpdatedBy { get; set; }

        /// <summary>
        /// 修改人姓名
        /// </summary>
        public string UpdatedUser { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
