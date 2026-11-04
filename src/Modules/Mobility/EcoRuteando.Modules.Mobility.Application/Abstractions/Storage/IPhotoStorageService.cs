namespace EcoRuteando.Modules.Mobility.Application.Abstractions.Storage;

/// <summary>
/// Resultado de una carga exitosa de fotografía de evidencia en el almacenamiento externo.
/// </summary>
public sealed record PhotoUploadResult(
    string Url,
    string FileName,
    string PublicId);

/// <summary>
/// Excepción controlada para errores del proveedor de almacenamiento de imágenes.
/// Contiene únicamente mensajes seguros aptos para el cliente sin exponer detalles internos ni credenciales.
/// </summary>
public sealed class PhotoStorageException : Exception
{
    public int StatusCode { get; }

    public PhotoStorageException(
        string safeClientMessage,
        int statusCode = 502,
        Exception? innerException = null)
        : base(safeClientMessage, innerException)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Abstracción para el almacenamiento seguro y persistente de fotografías de reportes de obstáculos (H-04 / CU07).
/// </summary>
public interface IPhotoStorageService
{
    /// <summary>
    /// Valida y sube una fotografía de evidencia al almacenamiento en la nube (carpeta ecoruteando/reports),
    /// devolviendo únicamente una URL HTTPS pública y un identificador único de archivo.
    /// </summary>
    Task<PhotoUploadResult> UploadReportPhotoAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        Guid? uploadedByUserId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca una fotografía recién subida como confirmada y asociada a un reporte persistido,
    /// impidiendo cualquier limpieza posterior.
    /// </summary>
    void MarkPhotoCommitted(string? photoUrl);

    /// <summary>
    /// Elimina una fotografía huérfana únicamente si se comprueba que fue subida en la operación actual
    /// por el mismo usuario y aún no ha sido asociada a ningún reporte. Nunca elimina imágenes históricas.
    /// </summary>
    Task<bool> TryDeleteUncommittedPhotoAsync(
        string? photoUrl,
        Guid userId,
        CancellationToken cancellationToken = default);
}
