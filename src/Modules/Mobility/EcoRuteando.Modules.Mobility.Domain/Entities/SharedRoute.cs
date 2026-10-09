using System.Text.Json;
using EcoRuteando.Shared.BaseClasses;
using EcoRuteando.Shared.Exceptions;

namespace EcoRuteando.Modules.Mobility.Domain.Entities;

/// <summary>
/// Registro de un recorrido compartido por un usuario
/// (tabla mobility.shared_routes). CU20 - "Compartir recorridos".
/// Solo se guarda la información que el usuario autorizó hacer pública.
/// </summary>
public sealed class SharedRoute : Entity<Guid>
{
    public Guid UsageId { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>
    /// Red social o medio elegido (WhatsApp, Facebook, X, enlace, etc.).
    /// </summary>
    public string? SocialNetwork { get; private set; }

    /// <summary>
    /// Datos públicos del recorrido (JSONB): distancia, duración, CO₂, etc.
    /// Se omiten los datos privados antes de publicar (RF32.5).
    /// </summary>
    public JsonDocument? SharedData { get; private set; }

    public bool Confirmed { get; private set; }

    private SharedRoute()
    {
    }

    public SharedRoute(
        Guid userId,
        Guid usageId,
        string? socialNetwork,
        JsonDocument? sharedData = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("El usuario es obligatorio.");

        if (usageId == Guid.Empty)
            throw new DomainException("El recorrido es obligatorio.");

        if (socialNetwork is not null && socialNetwork.Trim().Length > 50)
            throw new DomainException("La red social no puede superar 50 caracteres.");

        Id = Guid.NewGuid();
        UserId = userId;
        UsageId = usageId;
        SocialNetwork = string.IsNullOrWhiteSpace(socialNetwork) ? null : socialNetwork.Trim();
        SharedData = sharedData;
        Confirmed = true;
        CreatedAt = DateTime.UtcNow;
    }
}