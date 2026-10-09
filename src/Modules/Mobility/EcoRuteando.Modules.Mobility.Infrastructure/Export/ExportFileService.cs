using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using EcoRuteando.Modules.Mobility.Application.Abstractions.Export;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Export;

/// <summary>
/// Genera archivos de exportación sin librerías externas:
/// CSV con BOM UTF-8 (Excel), JSON indentado y XLSX (paquete OOXML
/// mínimo creado con System.IO.Compression).
/// </summary>
public sealed class ExportFileService : IExportFileService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { WriteIndented = true };

    public byte[] BuildCsv(params CsvTable[] tables)
    {
        var sb = new StringBuilder();

        for (var t = 0; t < tables.Length; t++)
        {
            if (t > 0)
            {
                sb.AppendLine();
            }

            AppendCsvCells(sb, tables[t].Headers);

            foreach (var row in tables[t].Rows)
            {
                AppendCsvCells(sb, row);
            }
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[preamble.Length + body.Length];

        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(body, 0, result, preamble.Length, body.Length);

        return result;
    }

    public byte[] BuildJson(object payload)
        => JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);

    public byte[] BuildXlsx(params XlsxSheet[] sheets)
    {
        using var stream = new MemoryStream();

        // El archivo XLSX es un paquete ZIP con XML (formato Office Open XML).
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(archive, "[Content_Types].xml", BuildContentTypes(sheets.Length));
            WriteEntry(archive, "_rels/.rels", BuildRootRelationships());
            WriteEntry(archive, "xl/workbook.xml", BuildWorkbook(sheets));
            WriteEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelationships(sheets.Length));

            for (var i = 0; i < sheets.Length; i++)
            {
                WriteEntry(
                    archive,
                    $"xl/worksheets/sheet{i + 1}.xml",
                    BuildWorksheet(sheets[i]));
            }
        }

        return stream.ToArray();
    }

    private static void AppendCsvCells(StringBuilder sb, IReadOnlyList<object> cells)
    {
        for (var i = 0; i < cells.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            sb.Append(EscapeCsv(FormatValue(cells[i])));
        }

        sb.AppendLine();
    }

    private static string EscapeCsv(string value)
    {
        var needsQuotes = value.Contains(',')
            || value.Contains(';')
            || value.Contains('"')
            || value.Contains('\r')
            || value.Contains('\n');

        if (!needsQuotes)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string BuildContentTypes(int sheetCount)
    {
        var sb = new StringBuilder()
            .AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>")
            .AppendLine("<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">")
            .AppendLine("  <Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>")
            .AppendLine("  <Default Extension=\"xml\" ContentType=\"application/xml\"/>")
            .AppendLine("  <Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>");

        for (var i = 1; i <= sheetCount; i++)
        {
            sb.Append("  <Override PartName=\"")
              .Append($"/xl/worksheets/sheet{i}.xml")
              .AppendLine("\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
        }

        sb.AppendLine("</Types>");

        return sb.ToString();
    }

    private static string BuildRootRelationships()
        => """
           <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
           <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
             <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
           </Relationships>
           """;

    private static string BuildWorkbook(IReadOnlyList<XlsxSheet> sheets)
    {
        var sb = new StringBuilder()
            .AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>")
            .AppendLine("<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">")
            .AppendLine("  <sheets>");

        for (var i = 0; i < sheets.Count; i++)
        {
            sb.Append("    <sheet name=\"")
              .Append(EscapeXmlAttribute(SanitizeSheetName(sheets[i].Name)))
              .Append("\" sheetId=\"")
              .Append(i + 1)
              .Append("\" r:id=\"rId")
              .Append(i + 1)
              .AppendLine("\"/>");
        }

        sb.AppendLine("  </sheets>")
          .AppendLine("</workbook>");

        return sb.ToString();
    }

    private static string BuildWorkbookRelationships(int sheetCount)
    {
        var sb = new StringBuilder()
            .AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>")
            .AppendLine("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");

        for (var i = 1; i <= sheetCount; i++)
        {
            sb.Append("  <Relationship Id=\"rId")
              .Append(i)
              .AppendLine("\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet" + i + ".xml\"/>");
        }

        sb.AppendLine("</Relationships>");

        return sb.ToString();
    }

    private static string BuildWorksheet(XlsxSheet sheet)
    {
        var sb = new StringBuilder()
            .AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>")
            .AppendLine("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">")
            .AppendLine("  <sheetData>");

        WriteRow(sb, 1, sheet.Headers.Select(h => (object)h!).ToList());

        for (var i = 0; i < sheet.Rows.Count; i++)
        {
            WriteRow(sb, i + 2, sheet.Rows[i]);
        }

        sb.AppendLine("  </sheetData>")
          .AppendLine("</worksheet>");

        return sb.ToString();
    }

    private static void WriteRow(
        StringBuilder sb,
        int rowIndex,
        IReadOnlyList<object> cells)
    {
        sb.Append("    <row r=\"").Append(rowIndex).Append("\">");

        for (var c = 0; c < cells.Count; c++)
        {
            var cellRef = $"{ColumnName(c)}{rowIndex}";
            var raw = cells[c];

            if (TryGetNumber(raw, out var number))
            {
                sb.Append("<c r=\"").Append(cellRef).Append("\"><v>")
                  .Append(number)
                  .Append("</v></c>");
            }
            else
            {
                sb.Append("<c r=\"").Append(cellRef).Append("\" t=\"inlineStr\"><is><t xml:space=\"preserve\">")
                  .Append(EscapeXmlText(FormatValue(raw)))
                  .Append("</t></is></c>");
            }
        }

        sb.AppendLine("</row>");
    }

    /// <summary>
    /// Convierte 0 → "A", 25 → "Z", 26 → "AA" ... (referencia de columna Excel).
    /// </summary>
    private static string ColumnName(int index)
    {
        var name = string.Empty;
        var n = index + 1;

        while (n > 0)
        {
            var remainder = (n - 1) % 26;
            name = (char)('A' + remainder) + name;
            n = (n - 1) / 26;
        }

        return name;
    }

    private static bool TryGetNumber(object? value, out string invariant)
    {
        invariant = string.Empty;

        switch (value)
        {
            case byte b: invariant = b.ToString(CultureInfo.InvariantCulture); return true;
            case short s: invariant = s.ToString(CultureInfo.InvariantCulture); return true;
            case int i: invariant = i.ToString(CultureInfo.InvariantCulture); return true;
            case long l: invariant = l.ToString(CultureInfo.InvariantCulture); return true;
            case float f: invariant = f.ToString("0.###", CultureInfo.InvariantCulture); return true;
            case double d: invariant = d.ToString("0.###", CultureInfo.InvariantCulture); return true;
            case decimal m: invariant = m.ToString("0.###", CultureInfo.InvariantCulture); return true;
            default: return false;
        }
    }

    private static string FormatValue(object? value)
        => value switch
        {
            null => string.Empty,
            string s => s,
            bool b => b ? "Sí" : "No",
            double d => d.ToString("0.###", CultureInfo.InvariantCulture),
            decimal m => m.ToString("0.###", CultureInfo.InvariantCulture),
            float f => f.ToString("0.###", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };

    private static string SanitizeSheetName(string name)
    {
        var sanitized = new string(name
            .Select(ch => "[]*?/\\".Contains(ch) ? ' ' : ch)
            .ToArray());

        return sanitized.Length > 31 ? sanitized[..31] : sanitized;
    }

    private static string EscapeXmlText(string value)
        => value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");

    private static string EscapeXmlAttribute(string value)
        => EscapeXmlText(value)
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");

    private static void WriteEntry(
        ZipArchive archive,
        string entryName,
        string content)
    {
        var entry = archive.CreateEntry(entryName);

        using var writer = new StreamWriter(
            entry.Open(),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(content);
    }
}