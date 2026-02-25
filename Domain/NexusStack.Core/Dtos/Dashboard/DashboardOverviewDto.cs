using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Dashboard
{
    public class DashboardOverviewDto
    {
        public DashboardStatisticsDataView Overview { get; set; } = new();
        public DashboardToDoItemsView ToDoItems { get; set; } = new();
    }

    public class DashboardStatisticsDataView
    {
        public DashboardStatisticsItem SalesAmount { get; set; } = new();
        public DashboardStatisticsItem OrdersCount { get; set; } = new();
        public DashboardStatisticsItem SalesCount { get; set; } = new();
        public DashboardStatisticsItem RefundAmount { get; set; } = new();
        public List<DashboardStatisticsChartItem> SalesAmountChart { get; set; } = [];
        public List<DashboardStatisticsChartItem> OrdersCountChart { get; set; } = [];
    }

    public class DashboardStatisticsChartItem
    {
        public DateTime? Date { get; set; }
        public decimal? Value { get; set; }
    }

    public class DashboardStatisticsItem
    {
        public decimal? ValueMonthly { get; set; }
        public decimal? ValueDaily { get; set; }
        public decimal? IncreaseRateMonthly { get; set; }
    }

    public class DashboardToDoItemsView
    {
        //待发货
        public int? PendingShipment { get; set; }
        //申报中
        public int? Declaring { get; set; }
        //申报失败
        public int? DeclaringFailed { get; set; }
        //待售后
        public int? PendingAfterSale { get; set; }
        //退货/退款
        public int? RefundOrReturn { get; set; }
        //物流已揽收
        public int? LogisticsPickedUp { get; set; }
        //超24小时未发货
        public int? Over24HoursUnshipped { get; set; }
        //超7天未签收
        public int? Over7DaysUnsigned { get; set; }
        //退货待签收
        public int? ReturnToReceive { get; set; }
    }
}
