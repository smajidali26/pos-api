using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace POSApi.Infrastructure.Services.Export;

public class CsvExportService : IExportService
{
    public byte[] ExportToCsv<T>(IEnumerable<T> data) where T : class
    {
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            Encoding = Encoding.UTF8
        });

        csvWriter.WriteRecords(data);
        streamWriter.Flush();

        return memoryStream.ToArray();
    }

    public byte[] ExportToCsv<T>(IEnumerable<T> data, Dictionary<string, string> columnMap) where T : class
    {
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            Encoding = Encoding.UTF8
        });

        // Write custom headers
        foreach (var header in columnMap.Values)
        {
            csvWriter.WriteField(header);
        }
        csvWriter.NextRecord();

        // Write data rows
        var properties = typeof(T).GetProperties()
            .Where(p => columnMap.ContainsKey(p.Name))
            .ToList();

        foreach (var item in data)
        {
            foreach (var propertyName in columnMap.Keys)
            {
                var property = properties.FirstOrDefault(p => p.Name == propertyName);
                var value = property?.GetValue(item);
                csvWriter.WriteField(value);
            }
            csvWriter.NextRecord();
        }

        streamWriter.Flush();
        return memoryStream.ToArray();
    }

    public byte[] ExportToPdf<T>(IEnumerable<T> data, string title, Dictionary<string, string> columns) where T : class
    {
        throw new NotImplementedException("Use PdfExportService for PDF exports");
    }
}
