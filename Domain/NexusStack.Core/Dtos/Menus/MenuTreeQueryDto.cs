using NexusStack.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Menus
{
    public class MenuTreeQueryDto : PagedQueryModelBase
    {
        /// <summary>
        /// 父级 Id
        /// </summary>
        public long ParentId { get; set; }

        ///// <summary>
        ///// 父级编号
        ///// </summary>
        //public string ParentCode { get; set; }

        /// <summary>
        /// 包含下级
        /// </summary>
        public bool IncludeChilds { get; set; }
    }
}
