using NexusStack.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Users
{
    public class UserTokenCacheDto : UserTokenDto
    {
        /// <summary>
        /// UserToken Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Token所属平台
        /// </summary>
        public PlatformType PlatformType { get; set; }
    }
}
