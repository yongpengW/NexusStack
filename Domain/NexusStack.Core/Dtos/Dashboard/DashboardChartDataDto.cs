using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Dashboard
{
    public class DashboardChartDataDto
    {
        public List<DashboardChartItem> Monthly { get; set; } = [];
        public List<DashboardChartItem> Yearly { get; set; } = [];
    }


    public class DashboardChartItem
    {
        public DateTimeOffset? Date { get; set; }
        public decimal? SalesAmount { get; set; }
        public int? Orders { get; set; }
    }
}
