using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Excel.Export
{
    public class PDFOrderInvoiceDto
    {
        public string Index { get; set; }
        public string Title { get; set; }

        public string? SubTitle { get; set; }

        public CachierInfo? CachierInfo { get; set; }

        public List<SaleItem>? SaleList { get; set; }

        public Summary? Summary { get; set; }

        public Logistics Logistics { get; set; }

        public string? Footer { get; set; }

        public string? QRCode { get; set; }

        public string? QRCodeDescription { get; set; }

        public string? path { get; set; }
    }
    public class CachierInfo
    {
        public string OrderId { get; set; }
        public string? PosName { get; set; }
        public string OrderTime { get; set; }

        public string? Cachier { get; set; }
    }

    public class SaleItem
    {
        public string? Name { get; set; }

        public string? Barcode { get; set; }

        public string Unit { get; set; }

        public int Count { get; set; }

        public string Total { get; set; }
    }
    public class Summary
    {
        public int TotalNum { get; set; }
        public string Total { get; set; }
        public string Discount { get; set; }
        public string Tax { get; set; }
        public string Payable { get; set; }
        public string Pay { get; set; }
    }
    public class Logistics
    {
        public string? Number { get; set; }
        public string? Receiver { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
