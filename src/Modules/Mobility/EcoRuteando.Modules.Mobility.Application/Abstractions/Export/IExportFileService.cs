namespace EcoRuteando.Modules.Mobility.Application.Abstractions.Export;

/// <summary>
/// Una tabla de valores para generar un archivo CSV (columna de encabezados + filas).
/// </summary>
public sealed record CsvTable(
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<object>> Rows);

/// <summary>
/// Una hoja de un libro Excel (XLSX).
/// </summary>
public sealed record XlsxSheet(
    string Name,
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<object>> Rows);

/// <summary>
/// Genera archivos de datos (CSV, JSON y XLSX) para la exportación de
/// historial y estadísticas (CU19). Sin dependencias externas:
/// XLSX se construye con System.IO.Compression (formato OOXML mínimo).
/// </summary>
public interface IExportFileService
{
    /// <summary>Genera un CSV con BOM UTF-8 (compatible con Excel) desde una o más tablas.</summary>
    byte[] BuildCsv(params CsvTable[] tables);

    /// <summary>Serializa un payload a JSON con formato indentado.</summary>
    byte[] BuildJson(object payload);

    /// <summary>Genera un libro XLSX (una hoja por tabla).</summary>
    byte[] BuildXlsx(params XlsxSheet[] sheets);
}