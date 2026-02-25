using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Regions
{
    public class RegionExportDto
    {
        public long Id { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int No { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 县
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// 镇
        /// </summary>
        public string Town { get; set; }

        /// <summary>
        /// 村
        /// </summary>
        public string Village { get; set; }

        /// <summary>
        /// 简称
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// 代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; }

        public RegionExportDto Clone()
        {
            return (RegionExportDto)MemberwiseClone();
        }
    }
}
