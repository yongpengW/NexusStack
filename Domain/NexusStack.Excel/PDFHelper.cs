using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Metadata;
using System.Text;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using iTextSharp.text;
using NexusStack.Excel.Export;
using iTextSharp.text.pdf;
using Document = iTextSharp.text.Document;
using Rectangle = iTextSharp.text.Rectangle;
using Image = iTextSharp.text.Image;
using Font = iTextSharp.text.Font;

namespace NexusStack.Excel
{
    public class PDFHelper
    {

        private float marginX = 25;
        private float marginY = 30;
        private float pageWidth = 150f;
        public static void PrintOrcer(PDFOrderInvoiceDto order)
        {
            BaseFont f_cn;
            string poath = "~/fonts/CALIBRI.TTF";

            f_cn = BaseFont.CreateFont(poath, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

            using (System.IO.FileStream fs = new FileStream("~/TempPdf" + "\\" + "download.pdf", FileMode.Create))
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                Paragraph p = new Paragraph();
                // Add meta information to the document
                document.AddAuthor("Mikael Blomquist");
                document.AddCreator("Sample application using iTestSharp");
                document.AddKeywords("PDF tutorial education");
                document.AddSubject("Document subject - Describing the steps creating a PDF document");
                document.AddTitle("The document title - Amplified Resource Group");
                // Open the document to enable you to write to the document
                document.Open();
                // Makes it possible to add text to a specific place in the document using 
                // a X & Y placement syntax.
                PdfContentByte cb = writer.DirectContent;
                cb.SetFontAndSize(f_cn, 16);
                // First we must activate writing
                cb.BeginText();
                // Add an image to a fixed position from disk
                iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance("~/images/arg.png");
                png.SetAbsolutePosition(200, 738);
                cb.AddImage(png);
                writeText(cb, "Header", 30, 718, f_cn, 14);
            }

        }

        private static void writeText(PdfContentByte cb, string Text, int X, int Y, BaseFont font, int Size)
        {
            cb.SetFontAndSize(font, Size);
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, Text, X, Y, 0);
        }

        public async Task<byte[]> CreatePdfInvoice(PDFOrderInvoiceDto order)
        {
            using (var stream = new MemoryStream())
            {
                var pageHeight = getOrderPDFHeight(order);
                var pageSize = new Rectangle(pageWidth, pageHeight);

                var document = new Document(pageSize, marginX, marginX, marginY, marginY);

                var writer = PdfWriter.GetInstance(document, stream);
                document.Open();
                var currentPosY = pageHeight;
                ElementPos ePos;

                //设置图片大小和位置
                var logoPath = Path.GetFullPath("images") + "\\logo.png";
                byte[] arr = File.ReadAllBytes(logoPath);
                Image image = Image.GetInstance(arr);
                image.ScaleToFit(document.PageSize.Width / 2, document.PageSize.Height / 2);
                ePos = getElementPos(image.ScaledWidth, image.ScaledHeight, currentPosY);
                image.SetAbsolutePosition(ePos.X, ePos.Y);
                currentPosY = ePos.N;
                //添加图片
                document.Add(image);

                //创建字体
                BaseFont baseFont = BaseFont.CreateFont("STSong-Light", "UniGB-UCS2-H", BaseFont.NOT_EMBEDDED);
                Font font = new Font(baseFont);

                //一个段落文本
                Paragraph paragraph = new Paragraph(order.Title, font);

                //添加段落
                document.Add(paragraph);

                //添加块到列
                var ct = new ColumnText(writer.DirectContent);

                ct.SetSimpleColumn(100, 150, 500, 800, 24, Element.ALIGN_LEFT);

                var chunk = new Chunk(order.Footer, font);

                ct.AddElement(chunk);
                ct.Go();


                document.Close();
                return stream.ToArray();
            }
        }


        private float getOrderPDFHeight(PDFOrderInvoiceDto order)
        {
            return 1000f;
        }

        private float getDeliveryPDFHeight()
        {
            return 600f;
        }

        private ElementPos getElementPos(float w, float h, float c, float gap = 0.2f)
        {
            var result = new ElementPos();
            result.X = (pageWidth - w / 2) / 2;
            result.Y = c - h;
            result.N = c - h - gap;
            return result;
        }

        class ElementPos
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float N { get; set; }
        }
    }
}
