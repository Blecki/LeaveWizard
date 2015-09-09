using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Drawing;

namespace LeaveWizard
{
    public partial class LeaveChart
    {
        private static String[] LongDayNames = new String[]
        {
            "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday"
        };

        public static void DrawSubSchedule(PdfSharp.Drawing.XGraphics Graphics, WeekData Week)
        {
            var basicPen = XPens.Black;
            var boldPen = new XPen(XColors.Black, 2);
            var font = new XFont("Times New Roman", 14, XFontStyle.Bold);

            var schedule = GenerateSubSchedule(Week);
            var width = Graphics.PageSize.Width;
            var height = Graphics.PageSize.Height;
            var horizontalMargin = 20;
            var horizontalSpan = width - (horizontalMargin * 2);
            var columnWidth = horizontalSpan / 8;
            var verticalMargin = 20;
            var topMargin = 50;
            var mainVerticalSpan = height - topMargin - (verticalMargin * 2);
            var rowCount = schedule.Subs.Count + schedule.OpenRoutes.Max(l => l.Count);
            var rowHeight = mainVerticalSpan / rowCount;
            if (rowHeight > 20) rowHeight = 20;
            var realBottom = verticalMargin + topMargin + (rowHeight * rowCount);
            var fontMargin = 5;

            for (double x = horizontalMargin; x <= width; x += columnWidth)
                Graphics.DrawLine(boldPen, x, verticalMargin, x, realBottom);

            Graphics.DrawLine(basicPen, horizontalMargin, verticalMargin, width - horizontalMargin, verticalMargin);

            for (double y = verticalMargin + topMargin; y <= realBottom; y += rowHeight)
                Graphics.DrawLine(basicPen, horizontalMargin, y, width - horizontalMargin, y);

            Graphics.DrawString("Relief Carrier", font, XBrushes.Black, new XRect(horizontalMargin, verticalMargin, columnWidth, topMargin), XStringFormats.TopCenter);
            Graphics.DrawString("Schedule", font, XBrushes.Black, new XRect(horizontalMargin, verticalMargin + 16, columnWidth, topMargin), XStringFormats.TopCenter);
            Graphics.DrawString(String.Format("PP {0} WK {1}", Week.PayPeriod, Week.Week), font, XBrushes.Black, new XRect(horizontalMargin, verticalMargin, columnWidth, topMargin), XStringFormats.BottomCenter);

            for (var day = 0; day < 7; ++day)
            {
                Graphics.DrawString(LongDayNames[day], font, XBrushes.Black, new XRect(horizontalMargin + (columnWidth * (day + 1)), verticalMargin, columnWidth, topMargin), XStringFormats.TopCenter);
                Graphics.DrawString((Week.SaturdayDate + TimeSpan.FromDays(day)).ToShortDateString(), font, XBrushes.Black, new XRect(horizontalMargin + (columnWidth * (day + 1)), verticalMargin, columnWidth, topMargin), XStringFormats.BottomCenter);
            }

            var row = 0;
            foreach (var sub in schedule.Subs)
            {
                Graphics.DrawString(sub.Name, font, XBrushes.Black, new XRect(fontMargin + horizontalMargin, verticalMargin + topMargin + (row * rowHeight), columnWidth, rowHeight), XStringFormats.Center);
                for (var c = 0; c < 7; ++c)
                    if (!String.IsNullOrEmpty(sub.Schedule[c]))
                        Graphics.DrawString(sub.Schedule[c], font, XBrushes.Black, new XRect(fontMargin + horizontalMargin + (columnWidth * (c + 1)), verticalMargin + topMargin + (row * rowHeight), columnWidth, rowHeight), XStringFormats.CenterLeft);
                row += 1;
            }

            for (var day = 0; day < 7; ++day)
            {
                var top = row;
                foreach (var entry in schedule.OpenRoutes[day])
                {
                    Graphics.DrawString(entry, font, XBrushes.Black, new XRect(fontMargin + horizontalMargin + (columnWidth * (day + 1)), verticalMargin + topMargin + (top * rowHeight), columnWidth, rowHeight), XStringFormats.CenterLeft);
                    top += 1;
                }
            }
        }
    }
}
