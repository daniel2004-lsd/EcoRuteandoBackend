using System.Collections.Concurrent;
using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EcoRuteando.Modules.Mobility.Application.Abstractions.Storage;
using EcoRuteando.Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Storage;

/// <summary>
/// Abstracción interna del cliente SDK de Cloudinary para permitir pruebas unitarias determinísticas
/// sin requerir credenciales reales ni tráfico de red en CI.
/// </summary>
public interface ICloudinarySdkClient
{
    bool IsConfigured { get; }

    Task<ImageUploadResult> UploadImageAsync(
        ImageUploadParams uploadParams,
        CancellationToken cancellationToken = default);

    Task<DeletionResult> DestroyImageAsync(
        DeletionParams deletionParams,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Adaptador oficial sobre CloudinaryDotNet.Cloudinary.
/// </summary>
public sealed class CloudinarySdkClientAdapter : ICloudinarySdkClient
{
    private readonly Cloudinary? _cloudinary;

    public bool IsConfigured => _cloudinary is not null;

    public CloudinarySdkClientAdapter(IOptions<CloudinaryOptions> options)
    {
        var cfg = options.Value;
        if (cfg.IsConfigured)
        {
            var account = new Account(
                cfg.CloudName.Trim(),
                cfg.ApiKey.Trim(),
                cfg.ApiSecret.Trim());

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
            var timeoutSeconds = Math.Clamp(
                cfg.TimeoutSeconds > 0 ? cfg.TimeoutSeconds : CloudinaryOptions.DefaultTimeoutSeconds,
                5,
                60);
            _cloudinary.Api.Timeout = timeoutSeconds * 1000;
        }
    }

    public async Task<ImageUploadResult> UploadImageAsync(
        ImageUploadParams uploadParams,
        CancellationToken cancellationToken = default)
    {
        if (_cloudinary is null)
        {
            throw new PhotoStorageException(
                "El almacenamiento de fotografías en la nube no está configurado.",
                503);
        }

        return await _cloudinary.UploadAsync(uploadParams, cancellationToken);
    }

    public async Task<DeletionResult> DestroyImageAsync(
        DeletionParams deletionParams,
        CancellationToken cancellationToken = default)
    {
        if (_cloudinary is null)
        {
            throw new PhotoStorageException(
                "El almacenamiento de fotografías en la nube no está configurado.",
                503);
        }

        return await _cloudinary.DestroyAsync(deletionParams);
    }
}

/// <summary>
/// Implementación de IPhotoStorageService respaldada por Cloudinary (H-04).
/// Valida tamaño, extensión, MIME y firma binaria real (magic bytes), sube la imagen mediante Stream
/// a la carpeta 'ecoruteando/reports', exige URL HTTPS y gestiona la limpieza segura de archivos huérfanos
/// pertenecientes exclusivamente a la operación actual.
/// </summary>
public sealed class CloudinaryPhotoStorageService : IPhotoStorageService
{
    private sealed record PendingPhotoOperation(
        string PublicId,
        Guid UserId,
        DateTime UploadedAtUtc);

    private static readonly ConcurrentDictionary<string, PendingPhotoOperation> UncommittedUploads =
        new(StringComparer.Ordinal);

    private static readonly TimeSpan UncommittedRetentionWindow = TimeSpan.FromMinutes(30);

    private readonly ICloudinarySdkClient _cloudinaryClient;
    private readonly CloudinaryOptions _options;
    private readonly ILogger<CloudinaryPhotoStorageService> _logger;

    public CloudinaryPhotoStorageService(
        ICloudinarySdkClient cloudinaryClient,
        IOptions<CloudinaryOptions> options,
        ILogger<CloudinaryPhotoStorageService> logger)
    {
        _cloudinaryClient = cloudinaryClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PhotoUploadResult> UploadReportPhotoAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        Guid? uploadedByUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null || !fileStream.CanRead)
        {
            throw new DomainException("El archivo de imagen es obligatorio.");
        }

        // Copiar a un MemoryStream acotado (máx. 5 MB + 1 byte para detectar exceso sin agotar memoria)
        using var boundedBuffer = new MemoryStream();
        var readBuffer = new byte[16 * 1024];
        long totalRead = 0;

        while (true)
        {
            var bytesRead = await fileStream.ReadAsync(
                readBuffer.AsMemory(0, readBuffer.Length),
                cancellationToken);

            if (bytesRead <= 0)
            {
                break;
            }

            totalRead += bytesRead;
            if (totalRead > ImageFileValidator.MaxFileSizeBytes)
            {
                throw new DomainException("El tamaño de la imagen no puede exceder 5 MB.");
            }

            boundedBuffer.Write(readBuffer, 0, bytesRead);
        }

        var metaValidation = ImageFileValidator.ValidateMetadata(
            totalRead,
            originalFileName,
            contentType);

        if (!metaValidation.IsValid)
        {
            throw new DomainException(metaValidation.ErrorMessage!);
        }

        var rawBytes = boundedBuffer.GetBuffer();
        var headerLength = (int)Math.Min(totalRead, 16);
        var signatureValidation = ImageFileValidator.ValidateSignature(
            rawBytes.AsSpan(0, headerLength),
            metaValidation.NormalizedExtension,
            contentType);

        if (!signatureValidation.IsValid)
        {
            throw new DomainException(signatureValidation.ErrorMessage!);
        }

        if (!_cloudinaryClient.IsConfigured || !_options.IsConfigured)
        {
            _logger.LogWarning(
                "Intento de subida de fotografía H-04 sin credenciales de Cloudinary configuradas (CLOUDINARY_CLOUD_NAME / CLOUDINARY_API_KEY / CLOUDINARY_API_SECRET).");

            throw new PhotoStorageException(
                "El servicio de almacenamiento de fotografías no está disponible en este momento.",
                503);
        }

        var uniqueId = Guid.NewGuid().ToString("N");
        var safeFileName = $"{uniqueId}{metaValidation.NormalizedExtension}";
        var folder = string.IsNullOrWhiteSpace(_options.Folder)
            ? CloudinaryOptions.DefaultFolder
            : _options.Folder.Trim().Trim('/');

        boundedBuffer.Position = 0;

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(safeFileName, boundedBuffer),
            Folder = folder,
            PublicId = uniqueId,
            UseFilename = false,
            UniqueFilename = false,
            Overwrite = false
        };

        var timeoutSeconds = Math.Clamp(
            _options.TimeoutSeconds > 0 ? _options.TimeoutSeconds : CloudinaryOptions.DefaultTimeoutSeconds,
            5,
            60);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        ImageUploadResult? uploadResult;
        try
        {
            uploadResult = await _cloudinaryClient.UploadImageAsync(uploadParams, timeoutCts.Token);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                "Tiempo de espera agotado ({TimeoutSeconds}s) al subir fotografía a Cloudinary.",
                timeoutSeconds);

            throw new PhotoStorageException(
                "Tiempo de espera agotado al almacenar la fotografía. Intente nuevamente.",
                504,
                ex);
        }
        catch (PhotoStorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Fallo de comunicación con el proveedor de almacenamiento de imágenes ({ExceptionType}).",
                ex.GetType().Name);

            throw new PhotoStorageException(
                "No fue posible almacenar la fotografía en este momento. Intente nuevamente más tarde.",
                502,
                ex);
        }

        if (uploadResult is null ||
            uploadResult.Error is not null ||
            (uploadResult.StatusCode != HttpStatusCode.OK && uploadResult.StatusCode != HttpStatusCode.Created))
        {
            _logger.LogError(
                "El proveedor de almacenamiento rechazó la carga de imagen con estado HTTP {StatusCode}.",
                uploadResult?.StatusCode);

            throw new PhotoStorageException(
                "No fue posible almacenar la fotografía en este momento. Intente nuevamente más tarde.",
                502);
        }

        var secureUri = uploadResult.SecureUrl;
        if (secureUri is null ||
            !secureUri.IsAbsoluteUri ||
            !string.Equals(secureUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("El proveedor de almacenamiento no devolvió una URL HTTPS válida.");

            throw new PhotoStorageException(
                "El proveedor de almacenamiento no devolvió una URL HTTPS segura.",
                502);
        }

        var resolvedPublicId = !string.IsNullOrWhiteSpace(uploadResult.PublicId)
            ? uploadResult.PublicId
            : $"{folder}/{uniqueId}";

        var httpsUrl = secureUri.AbsoluteUri;

        PurgeExpiredUncommittedEntries();
        UncommittedUploads[httpsUrl] = new PendingPhotoOperation(
            resolvedPublicId,
            uploadedByUserId ?? Guid.Empty,
            DateTime.UtcNow);

        return new PhotoUploadResult(
            Url: httpsUrl,
            FileName: safeFileName,
            PublicId: resolvedPublicId);
    }

    public void MarkPhotoCommitted(string? photoUrl)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            return;
        }

        UncommittedUploads.TryRemove(photoUrl.Trim(), out _);
    }

    public async Task<bool> TryDeleteUncommittedPhotoAsync(
        string? photoUrl,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(photoUrl) || userId == Guid.Empty)
        {
            return false;
        }

        var normalizedUrl = photoUrl.Trim();

        // Comprobar estrictamente que la imagen pertenece a la operación actual no confirmada y al mismo usuario
        if (!UncommittedUploads.TryGetValue(normalizedUrl, out var pending))
        {
            return false;
        }

        if (pending.UserId != userId)
        {
            return false;
        }

        var expectedPrefix = (string.IsNullOrWhiteSpace(_options.Folder)
            ? CloudinaryOptions.DefaultFolder
            : _options.Folder.Trim().Trim('/')) + "/";

        if (!pending.PublicId.StartsWith(expectedPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        if (!UncommittedUploads.TryRemove(normalizedUrl, out _))
        {
            return false;
        }

        if (!_cloudinaryClient.IsConfigured || !_options.IsConfigured)
        {
            return false;
        }

        try
        {
            var deletionResult = await _cloudinaryClient.DestroyImageAsync(
                new DeletionParams(pending.PublicId)
                {
                    ResourceType = ResourceType.Image,
                    Invalidate = true
                },
                cancellationToken);

            return deletionResult is not null &&
                   string.Equals(deletionResult.Result, "ok", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "No se pudo eliminar la fotografía huérfana de la operación actual ({ExceptionType}).",
                ex.GetType().Name);
            return false;
        }
    }

    private static void PurgeExpiredUncommittedEntries()
    {
        var cutoff = DateTime.UtcNow - UncommittedRetentionWindow;
        foreach (var kvp in UncommittedUploads)
        {
            if (kvp.Value.UploadedAtUtc < cutoff)
            {
                UncommittedUploads.TryRemove(kvp.Key, out _);
            }
        }
    }
}
