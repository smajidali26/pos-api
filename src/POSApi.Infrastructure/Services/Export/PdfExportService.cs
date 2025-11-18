using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace POSApi.Infrastructure.Services.Export;

public class PdfExportService : IExportService
{
    public byte[] ExportToPdf<T>(IEnumerable<T> data, string title, Dictionary<string, string> columns) where T : class
    {
        using var memoryStream = new MemoryStream();
        using var writer = new PdfWriter(memoryStream);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        // Add title
        var titleParagraph = new Paragraph(title)
            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
            .SetFontSize(18)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(20);
        document.Add(titleParagraph);

        // Add generation date
        var dateParagraph = new Paragraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC")
            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
            .SetFontSize(10)
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetMarginBottom(10);
        document.Add(dateParagraph);

        // Create table
        var table = new Table(UnitValue.CreatePercentArray(columns.Count))
            .UseAllAvailableWidth()
            .SetMarginTop(10);

        // Add header row
        foreach (var columnHeader in columns.Values)
        {
            var headerCell = new Cell()
                .Add(new Paragraph(columnHeader)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(10))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5);
            table.AddHeaderCell(headerCell);
        }

        // Add data rows
        var properties = typeof(T).GetProperties()
            .Where(p => columns.ContainsKey(p.Name))
            .ToList();

        var dataList = data.ToList();
        foreach (var item in dataList)
        {
            foreach (var propertyName in columns.Keys)
            {
                var property = properties.FirstOrDefault(p => p.Name == propertyName);
                var value = property?.GetValue(item);
                var displayValue = FormatValue(value);

                var cell = new Cell()
                    .Add(new Paragraph(displayValue)
                        .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                        .SetFontSize(9))
                    .SetPadding(5);
                table.AddCell(cell);
            }
        }

        document.Add(table);

        // Add footer with total count
        var footerParagraph = new Paragraph($"Total Records: {dataList.Count}")
            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
            .SetFontSize(10)
            .SetMarginTop(20);
        document.Add(footerParagraph);

        document.Close();

        return memoryStream.ToArray();
    }

    public byte[] ExportToCsv<T>(IEnumerable<T> data) where T : class
    {
        throw new NotImplementedException("Use CsvExportService for CSV exports");
    }

    public byte[] ExportToCsv<T>(IEnumerable<T> data, Dictionary<string, string> columnMap) where T : class
    {
        throw new NotImplementedException("Use CsvExportService for CSV exports");
    }

    private string FormatValue(object? value)
    {
        if (value == null) return string.Empty;

        return value switch
        {
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            decimal dec => dec.ToString("N2"),
            double dbl => dbl.ToString("N2"),
            float flt => flt.ToString("N2"),
            bool b => b ? "Yes" : "No",
            _ => value.ToString() ?? string.Empty
        };
    }
}
