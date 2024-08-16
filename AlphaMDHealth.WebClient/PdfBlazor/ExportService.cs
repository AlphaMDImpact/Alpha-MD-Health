//using AlphaMDHealth.Model.Models.Common;
//using System.ComponentModel;
//using Syncfusion.Pdf;
//using Syncfusion.Pdf.Graphics;
//using System.ComponentModel;
//using Color = Syncfusion.Drawing.Color;
//using PointF = Syncfusion.Drawing.PointF;
//using SizeF = Syncfusion.Drawing.SizeF;

//namespace AlphaMDHealth.WebClient.PdfBlazor
//{
//    public class ExportService
//    {
//        public static MemoryStream CreatePDF(string FileName, List<string> OrgDetail, Dictionary<string, string> Header, List<TableModel> tableData)
//        {
//            PdfDocument pdfDocument = new PdfDocument();
//            PdfPage page = pdfDocument.Pages.Add();
//            PdfGraphics graphics = page.Graphics;
//            SizeF clientSize = page.GetClientSize();

//            float pageWidth = clientSize.Width;
//            float pageHeight = clientSize.Height;
//            float startX = 0;

//            // Organization Logo
//            byte[] logoBytes = Convert.FromBase64String(OrgDetail[0]);
//            PdfBitmap logoBitmap = new PdfBitmap(new MemoryStream(logoBytes));
//            graphics.DrawImage(logoBitmap, new Syncfusion.Drawing.RectangleF(startX, 0, 60, 60));
//            PointF iconLocation = new PointF(14, 13);




//            float orgDetailsY = 70;


//            float contentWidth = pageWidth;
//            float keyXLeft = startX;
//            float keyXRight = (startX + contentWidth / 2) + 170;
//            float valueXLeft = startX;
//            float valueXRight = (startX + contentWidth / 2) + 170;

//            // Header Section
//            var headerPairs = Header.ToList();
//            for (int i = 0; i < headerPairs.Count; i += 2)
//            {
//                var pair1 = headerPairs[i];
//                var pair2 = i + 1 < headerPairs.Count ? headerPairs[i + 1] : new KeyValuePair<string, string>();

//                var boldFont = new PdfStandardFont(PdfFontFamily.Helvetica, 14, PdfFontStyle.Bold);

//                graphics.DrawString(pair1.Key, boldFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(keyXLeft, orgDetailsY));
//                graphics.DrawString(pair2.Key, boldFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(keyXRight, orgDetailsY));

//                orgDetailsY += 20;

//                graphics.DrawString(pair1.Value, new PdfStandardFont(PdfFontFamily.Helvetica, 10), PdfBrushes.Black, new Syncfusion.Drawing.PointF(valueXLeft, orgDetailsY));
//                graphics.DrawString(pair2.Value, new PdfStandardFont(PdfFontFamily.Helvetica, 10), PdfBrushes.Black, new Syncfusion.Drawing.PointF(valueXRight, orgDetailsY));

//                orgDetailsY += 20;
//            }

//            float tableY = orgDetailsY + 40;

//            CreateTable<TableModel>(graphics, tableData, tableY, startX, contentWidth);

//            MemoryStream stream = new MemoryStream();
//            pdfDocument.Save(stream);
//            pdfDocument.Close(true);
//            stream.Position = 0;

//            return stream;
//        }

//        private static float CreateTable<T>(PdfGraphics graphics, IList<T> data, float startY, float startX, float contentWidth)
//        {
//            float cellHeight = 20;
//            int columnCount = TypeDescriptor.GetProperties(typeof(T)).Count;
//            float cellWidth = contentWidth / columnCount;

//            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

//            // Draw table headers and borders separately
//            for (int i = 0; i < columnCount; i++)
//            {
//                PropertyDescriptor prop = props[i];
//                float cellX = startX + i * cellWidth;
//                string cellText = prop.Name;
//                PdfFont headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 14, PdfFontStyle.Bold);

//                graphics.DrawRectangle(PdfPens.Black, new Syncfusion.Drawing.RectangleF(cellX, startY, cellWidth, cellHeight));

//                graphics.DrawString(cellText, headerFont, PdfBrushes.Black, new Syncfusion.Drawing.RectangleF(cellX, startY, cellWidth, cellHeight));

//                if (i < columnCount - 1)
//                {
//                    graphics.DrawLine(PdfPens.Black, cellX + cellWidth, startY, cellX + cellWidth, startY + cellHeight);
//                }
//            }

//            startY += cellHeight;

//            for (int row = 0; row < data.Count; row++)
//            {

//                graphics.DrawRectangle(PdfPens.Black, new Syncfusion.Drawing.RectangleF(startX, startY, contentWidth, cellHeight));

//                for (int i = 0; i < columnCount; i++)
//                {
//                    PropertyDescriptor prop = props[i];
//                    float cellX = startX + i * cellWidth;
//                    string cellText = prop.GetValue(data[row]).ToString();

//                    graphics.DrawString(cellText, new PdfStandardFont(PdfFontFamily.Helvetica, 12), PdfBrushes.Black, new Syncfusion.Drawing.RectangleF(cellX, startY, cellWidth, cellHeight));

//                    if (i < columnCount - 1)
//                    {
//                        graphics.DrawLine(PdfPens.Black, cellX + cellWidth, startY, cellX + cellWidth, startY + cellHeight);
//                    }
//                }

//                startY += cellHeight;

//                graphics.DrawLine(PdfPens.Black, startX, startY, startX + contentWidth, startY);
//            }

//            return startY;
//        }

//    }
//}
