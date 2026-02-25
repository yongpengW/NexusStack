using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusStack.Core.Entities.Schedules;
using NexusStack.EFCore.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Mapping
{
    public class ScheduleTaskMapping : MapBase<SeedDataTask>
    {
        public override void Configure(EntityTypeBuilder<SeedDataTask> builder)
        {
            base.Configure(builder);

            // 可通过此设置DateTime类型的精度，但是不能超过数据库的精度6
            //builder.Property(x => x.LastWriteTime).HasPrecision(7);
        }
    }
}
