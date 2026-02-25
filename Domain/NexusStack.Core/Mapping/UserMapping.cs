using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusStack.Core.Entities.Users;
using NexusStack.EFCore.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Mapping
{
    public class UserMapping : MapBase<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);

            // 一个用户下可以有多个角色（一对多的关系）
            builder.HasMany(a => a.Roles).WithMany(a => a.Users)
                .UsingEntity<UserRole>();
        }
    }
}
