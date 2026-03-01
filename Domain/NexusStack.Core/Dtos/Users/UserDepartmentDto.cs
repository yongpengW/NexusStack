using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Users
{
    public class UserDepartmentDto
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 部门 Id
        /// 可以是RegionId也可以是ShopId
        /// </summary>
        public long DepartmentId { get; set; }
    }
}
