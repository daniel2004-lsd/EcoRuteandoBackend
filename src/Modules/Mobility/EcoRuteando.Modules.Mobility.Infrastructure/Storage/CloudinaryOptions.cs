namespace EcoRuteando.Modules.Mobility.Infrastructure.Storage;

/// <summary>
/// Configuración tipada para la integración con Cloudinary (H-04).
/// Se enlaza desde las variables de entorno CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY y CLOUDINARY_API_SECRET
/// (o la sección de configuración Cloudinary). Nunca debe contener secretos cableados en código o JSON.
/// </summary>
public sealed class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";
    public const string DefaultFolder = "ecoruteando/reports";
    public const int DefaultTimeoutSeconds = 15;

    public string CloudName { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string ApiSecret { get; set; } = string.Empty;

    public string Folder { get; set; } = DefaultFolder;

    public int TimeoutSeconds { get; set; } = DefaultTimeoutSeconds;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(CloudName) &&
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(ApiSecret);
}
