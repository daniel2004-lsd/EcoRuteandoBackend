using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Shared.BaseClasses;
using EcoRuteando.Shared.Exceptions;
using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Mobility.Domain.Entities;

/// <summary>
/// Reporte ciudadano de un obstáculo o problema en las rutas
/// (tabla community.obstacle_reports). Nace en estado 'pending' y queda
/// en espera de validación por parte de un administrador.
/// </summary>
public sealed class ObstacleReport : Entity<Guid>
{
    public Guid UserId { get; private set; }

    public string ReportType { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Point Location { get; private set; } = default!;

    public string? AddressText { get; private set; }

    public string? PhotoUrl { get; private set; }

    public ReportStatus Status { get; private set; }

    public Guid? ValidatorId { get; private set; }

    public string? ValidationNote { get; private set; }

    public DateTime? ValidatedAt { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? ExpiresAt { get; private set; }

    private ObstacleReport()
    {
    }

    public ObstacleReport(
        Guid userId,
        string reportType,
        string description,
        Point location,
        string? addressText = null,
        string? photoUrl = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("El usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(reportType))
            throw new DomainException("El tipo de obstáculo es obligatorio.");

        if (reportType.Trim().Length > 100)
            throw new DomainException("El tipo no puede superar 100 caracteres.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("La descripción es obligatoria.");

        if (description.Trim().Length > 2000)
            throw new DomainException("La descripción no puede superar 2000 caracteres.");

        if (location is null)
            throw new DomainException("La ubicación del obstáculo es obligatoria.");

        Id = Guid.NewGuid();
        UserId = userId;
        ReportType = reportType.Trim();
        Description = description.Trim();
        Location = location;
        AddressText = string.IsNullOrWhiteSpace(addressText) ? null : addressText.Trim();
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
        Status = ReportStatus.Pending;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validación administrativa del reporte: queda 'validated' o 'rejected'.
    /// </summary>
    public void Validate(
        Guid validatorId,
        ReportStatus newStatus,
        string? validationNote = null)
    {
        if (validatorId == Guid.Empty)
            throw new DomainException("El validador es obligatorio.");

        if (validatorId == UserId)
            throw new DomainException("Un usuario no puede validar su propio reporte.");

        if (newStatus is not (ReportStatus.Validated or ReportStatus.Rejected))
            throw new DomainException("El reporte solo puede quedar validado o rechazado.");

        if (validationNote is not null && validationNote.Trim().Length > 1000)
            throw new DomainException("La nota de validación no puede superar 1000 caracteres.");

        ValidatorId = validatorId;
        ValidationNote = string.IsNullOrWhiteSpace(validationNote) ? null : validationNote.Trim();
        ValidatedAt = DateTime.UtcNow;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}