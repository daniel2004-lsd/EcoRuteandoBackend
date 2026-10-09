namespace EcoRuteando.Modules.Mobility.Application.Exports;

/// <summary>
/// Resultado de una exportación: contenido binario listo para descargar.
/// </summary>
public sealed record ExportFileResponse(
    byte[] Content,
    string ContentType,
    string FileName);