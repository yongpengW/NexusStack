using NexusStack.Infrastructure.SnowFlake;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.EFCore.Entities
{
    public abstract class EntityBase : EntityBase<long>
    {
        public EntityBase()
            : base(SnowFlake.Instance.NextId())
        {
        }

        public override long Id { get => base.Id; set => base.Id = value; }
    }

    /// <summary>
    /// 抽象化数据实体基类，添加了软删除的功能
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class EntityBase<TKey> : Entity<TKey>, ISoftDelete
    {
        protected EntityBase(TKey id)
        {
            this.Id = id;
        }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}
