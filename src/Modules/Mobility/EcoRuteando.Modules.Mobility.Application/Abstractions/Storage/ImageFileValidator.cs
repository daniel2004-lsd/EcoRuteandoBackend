namespace EcoRuteando.Modules.Mobility.Application.Abstractions.Storage;

/// <summary>
///Resultado de validación de archivo de imagen (extensión, MIME, tamaño y firma mágica binaria).
/// </summary>
public sealed record ImageValidationResult(
    bool IsValid,
    string? ErrorMessage = null,
    string NormalizedExtension = "");

/// <summary>
/// Validador defensivo de imágenes para reportes de obstáculos (H-04).
/// Verifica tamaño (máx. 5 MB), extensión, tipo MIME y firma binaria real (magic bytes) para JPEG, PNG y WEBP.
/// </summary>
public static class ImageFileValidator
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    // PNG: 89 50 4E 47 0D 0A 1A 0A
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static ImageValidationResult ValidateMetadata(
        long fileLength,
        string? fileName,
        string? contentType)
    {
        if (fileLength <= 0)
        {
            return new ImageValidationResult(false, "El archivo de imagen es obligatorio.");
        }

        if (fileLength > MaxFileSizeBytes)
        {
            return new ImageValidationResult(false, "El tamaño de la imagen no puede exceder 5 MB.");
        }

        var ext = Path.GetExtension(fileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            return new ImageValidationResult(
                false,
                "Formato de imagen no soportado. Formatos válidos: JPG, PNG, WEBP.");
        }

        if (string.IsNullOrWhiteSpace(contentType) || !AllowedMimeTypes.Contains(contentType))
        {
            return new ImageValidationResult(
                false,
                "El tipo de contenido no corresponde a una imagen válida.");
        }

        var normalizedExt = ext.ToLowerInvariant();
        var normalizedMime = contentType.Trim().ToLowerInvariant();

        bool mimeMatchesExtension =
            (normalizedMime == "image/jpeg" && (normalizedExt == ".jpg" || normalizedExt == ".jpeg")) ||
            (normalizedMime == "image/png" && normalizedExt == ".png") ||
            (normalizedMime == "image/webp" && normalizedExt == ".webp");

        if (!mimeMatchesExtension)
        {
            return new ImageValidationResult(
                false,
                "El tipo de contenido no coincide con la extensión del archivo.");
        }

        return new ImageValidationResult(true, null, normalizedExt);
    }

    public static ImageValidationResult ValidateSignature(
        ReadOnlySpan<byte> headerBytes,
        string normalizedExtension,
        string contentType)
    {
        if (headerBytes.IsEmpty)
        {
            return new ImageValidationResult(false, "El archivo de imagen es obligatorio.");
        }

        var normalizedMime = contentType.Trim().ToLowerInvariant();

        if (normalizedMime == "image/jpeg" && (normalizedExtension == ".jpg" || normalizedExtension == ".jpeg"))
        {
            // JPEG SOI + marker: FF D8 FF
            if (headerBytes.Length >= 3 &&
                headerBytes[0] == 0xFF &&
                headerBytes[1] == 0xD8 &&
                headerBytes[2] == 0xFF)
            {
                return new ImageValidationResult(true, null, normalizedExtension);
            }

            return new ImageValidationResult(
                false,
                "El contenido binario del archivo no corresponde a una imagen JPEG válida.");
        }

        if (normalizedMime == "image/png" && normalizedExtension == ".png")
        {
            if (headerBytes.Length >= PngSignature.Length &&
                headerBytes[..PngSignature.Length].SequenceEqual(PngSignature))
            {
                return new ImageValidationResult(true, null, normalizedExtension);
            }

            return new ImageValidationResult(
                false,
                "El contenido binario del archivo no corresponde a una imagen PNG válida.");
        }

        if (normalizedMime == "image/webp" && normalizedExtension == ".webp")
        {
            // RIFF (52 49 46 46) + 4 bytes size + WEBP (57 45 42 50)
            if (headerBytes.Length >= 12 &&
                headerBytes[0] == 0x52 && headerBytes[1] == 0x49 && headerBytes[2] == 0x46 && headerBytes[3] == 0x46 &&
                headerBytes[8] == 0x57 && headerBytes[9] == 0x45 && headerBytes[10] == 0x42 && headerBytes[11] == 0x50)
            {
                return new ImageValidationResult(true, null, normalizedExtension);
            }

            return new ImageValidationResult(
                false,
                "El contenido binario del archivo no corresponde a una imagen WEBP válida.");
        }

        return new ImageValidationResult(
            false,
            "Formato de imagen no soportado. Formatos válidos: JPG, PNG, WEBP.");
    }
}
