using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Users
{
    public class UserDepartmentDto
    {
        /// <summary>
        /// 部门 Id
        /// 可以是RegionId也可以是ShopId
        /// </summary>
        public long DepartmentId { get; set; }

        /// <summary>
        /// 部门名称
        /// 可以是RegionName也可以是ShopName
        /// </summary>
        public string? DepartmentName { get; set; }
    }
}
