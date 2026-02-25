using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Core.Dtos.Dashboard
{
    public class DashboardHotSalesDto
    {
        public List<DashboardHotSaleProduct> Monthly { get; set; } = [];
        public List<DashboardHotSaleProduct> Quarterly { get; set; } = [];
        public List<DashboardHotSaleProduct> Total { get; set; } = [];
    }

    public class DashboardHotSaleProduct
    {
        public string? ProductName { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public string? Sku { get; set; }
        public string? MychemId { get; set; }
        public string? Plu { get; set; }
        public string? Barcode { get; set; }
        public int? Quantity { get; set; }
    }
}
