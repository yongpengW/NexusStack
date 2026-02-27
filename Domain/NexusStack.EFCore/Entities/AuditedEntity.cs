using NexusStack.Infrastructure.SnowFlake;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.EFCore.Entities
{
    public abstract class AuditedEntity : AuditedEntity<long>
    {
        public AuditedEntity()
            : base(SnowFlake.Instance.NextId())
        {
        }

        public override long Id { get => base.Id; set => base.Id = value; }
    }

    /// <summary>
    /// 拥有IsDelete软删除标识，CreatedAt CreateBy、UpdatedAt UpdateBy 主键可传递类型
    /// </summary>
    public abstract class AuditedEntity<TKey> : EntityBase<TKey>, IAuditedEntity
    {
        protected AuditedEntity(TKey id)
            : base(id)
        {
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// 创建人id
        /// </summary>
        public long? CreatedBy { get; set; } = 0;

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// 修改人id
        /// </summary>
        public long? UpdatedBy { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
