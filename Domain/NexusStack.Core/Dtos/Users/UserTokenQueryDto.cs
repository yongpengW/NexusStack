using NexusStack.Infrastructure.Enums;
using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Users
{
    public class UserTokenQueryDto : PagedQueryModelBase
    {
        /// <summary>
        /// 所属平台
        /// </summary>
        public PlatformType platformType { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }
    }
}
