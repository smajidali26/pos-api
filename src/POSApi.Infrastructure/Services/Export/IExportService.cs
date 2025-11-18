namespace POSApi.Infrastructure.Services.Export;

/// <summary>
/// Generic export service interface for PDF and CSV generation
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Export data to PDF format
    /// </summary>
    /// <typeparam name="T">Type of data to export</typeparam>
    /// <param name="data">Collection of data items</param>
    /// <param name="title">Report title</param>
    /// <param name="columns">Column definitions (property name, display name)</param>
    /// <returns>PDF file as byte array</returns>
    byte[] ExportToPdf<T>(IEnumerable<T> data, string title, Dictionary<string, string> columns) where T : class;

    /// <summary>
    /// Export data to CSV format
    /// </summary>
    /// <typeparam name="T">Type of data to export</typeparam>
    /// <param name="data">Collection of data items</param>
    /// <returns>CSV file as byte array</returns>
    byte[] ExportToCsv<T>(IEnumerable<T> data) where T : class;

    /// <summary>
    /// Export data to CSV with custom column mapping
    /// </summary>
    /// <typeparam name="T">Type of data to export</typeparam>
    /// <param name="data">Collection of data items</param>
    /// <param name="columnMap">Column mappings (property name, CSV header)</param>
    /// <returns>CSV file as byte array</returns>
    byte[] ExportToCsv<T>(IEnumerable<T> data, Dictionary<string, string> columnMap) where T : class;
}
