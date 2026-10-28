using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Shared.Exceptions;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility.Entities;

public sealed class RouteUsageTests
{
    private readonly Guid _validUserId = Guid.NewGuid();
    private readonly Guid _validRouteId = Guid.NewGuid();

    // ==========================================
    // 1. Constructor Tests
    // ==========================================

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsDomainException()
    {
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            new RouteUsage(Guid.Empty, _validRouteId, TransportType.Bike, "web"));

        Assert.Equal("El usuario es obligatorio.", ex.Message);
    }

    [Fact]
    public void Constructor_WithEmptyRouteId_ThrowsDomainException()
    {
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            new RouteUsage(_validUserId, Guid.Empty, TransportType.Bike, "web"));

        Assert.Equal("La ruta es obligatoria.", ex.Message);
    }

    [Fact]
    public void Constructor_WithInvalidSource_ThrowsDomainException()
    {
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            new RouteUsage(_validUserId, _validRouteId, TransportType.Bike, "invalid_source_123"));

        Assert.Contains("El origen 'invalid_source_123' no es válido.", ex.Message);
    }

    [Theory]
    [InlineData("web", UsageSource.Web)]
    [InlineData("mobile", UsageSource.Mobile)]
    [InlineData("pwa", UsageSource.Pwa)]
    public void Constructor_WithValidSource_InitializesPropertiesCorrectly(string sourceStr, UsageSource expectedSource)
    {
        // Arrange
        var startedAt = DateTime.UtcNow.AddMinutes(-10);

        // Act
        var usage = new RouteUsage(
            _validUserId,
            _validRouteId,
            TransportType.Bike,
            sourceStr,
            startedAt: startedAt);

        // Assert
        Assert.NotEqual(Guid.Empty, usage.Id);
        Assert.Equal(_validUserId, usage.UserId);
        Assert.Equal(_validRouteId, usage.RouteId);
        Assert.Equal(TransportType.Bike, usage.TransportMode);
        Assert.Equal(expectedSource, usage.Source);
        Assert.Equal(startedAt, usage.StartedAt);
        Assert.False(usage.Completed);
        Assert.False(usage.Interrupted);
        Assert.Null(usage.EndedAt);
        Assert.Null(usage.ActualDistanceKm);
        Assert.Null(usage.ActualDurationMin);
        Assert.Null(usage.ActualCo2Kg);
    }

    // ==========================================
    // 2. Complete() Tests
    // ==========================================

    [Fact]
    public void Complete_WithValidMetrics_SetsPropertiesAndMarksCompleted()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddMinutes(-30);
        var usage = new RouteUsage(_validUserId, _validRouteId, TransportType.Bike, "mobile", startedAt: startTime);
        var endTime = DateTime.UtcNow;

        // Act
        usage.Complete(
            actualDistanceKm: 12.5m,
            actualDurationMin: 28,
            actualCo2Kg: 1.85m,
            endedAt: endTime);

        // Assert
        Assert.True(usage.Completed);
        Assert.False(usage.Interrupted);
        Assert.Equal(12.5m, usage.ActualDistanceKm);
        Assert.Equal(28, usage.ActualDurationMin);
        Assert.Equal(1.85m, usage.ActualCo2Kg);
        Assert.Equal(endTime, usage.EndedAt);
    }

    [Fact]
    public void Complete_WithNegativeDistance_ThrowsDomainException()
    {
        // Arrange
        var usage = new RouteUsage(_validUserId, _validRouteId, TransportType.Car, "web");

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            usage.Complete(actualDistanceKm: -1m, actualDurationMin: 10, actualCo2Kg: 0.5m, endedAt: DateTime.UtcNow));

        Assert.Equal("La distancia real no puede ser negativa.", ex.Message);
    }

    [Fact]
    public void Complete_WithNegativeDuration_ThrowsDomainException()
    {
        // Arrange
        var usage = new RouteUsage(_validUserId, _validRouteId, TransportType.Car, "web");

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            usage.Complete(actualDistanceKm: 5m, actualDurationMin: -5, actualCo2Kg: 0.5m, endedAt: DateTime.UtcNow));

        Assert.Equal("La duración real no puede ser negativa.", ex.Message);
    }

    [Fact]
    public void Complete_WithEndedAtBeforeStartedAt_ThrowsDomainException()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var usage = new RouteUsage(_validUserId, _validRouteId, TransportType.Walking, "web", startedAt: startTime);
        var invalidEndTime = startTime.AddMinutes(-10);

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            usage.Complete(actualDistanceKm: 2m, actualDurationMin: 15, actualCo2Kg: 0m, endedAt: invalidEndTime));

        Assert.Equal("La fecha de fin no puede ser anterior al inicio.", ex.Message);
    }

    [Fact]
    public void Complete_WhenAlreadyEnded_ReturnsWithoutThrowing_Idempotent()
    {
        // Arrange
        var usage = new RouteUsage(_validUserId, _validRouteId, TransportType.Bike, "web", startedAt: DateTime.UtcNow.AddMinutes(-15));
        var firstEndTime = DateTime.UtcNow.AddMinutes(-5);
        usage.Complete(10m, 20, 1.5m, firstEndTime);

        // Act: Llamar a Complete por segunda vez (comportamiento idempotente preexistente)
        usage.Complete(99m, 999, 99m, DateTime.UtcNow);

        // Assert: Métricas originales preservadas sin lanzar excepción
        Assert.True(usage.Completed);
        Assert.Equal(10m, usage.ActualDistanceKm);
        Assert.Equal(20, usage.ActualDurationMin);
        Assert.Equal(1.5m, usage.ActualCo2Kg);
        Assert.Equal(firstEndTime, usage.EndedAt);
    }

    [Fact]
    public void Complete_DoesNotValidateNegativeCo2_DocumentsDomainAsymmetry()
    {
        // Note: RouteUsage.Complete no valida `actualCo2Kg < 0` a diferencia de Interrupt,
        // permitiendo que pase sin excepción a nivel de entidad.
        // Arrange
        var usage = new RouteUsage(_validUserId, _validRouteId, TransportType.Car, "web");

        // Act & Assert: No lanza excepción en Complete
        usage.Complete(actualDistanceKm: 5m, actualDurationMin: 15, actualCo2Kg: -0.5m, endedAt: DateTime.UtcNow);
        Assert.Equal(-0.5m, usage.ActualCo2Kg);
    }

    // ==========================================
    // 3. Interrupt() Tests
    // ==========================================

    [Fact]
    public void Interrupt_WithValidMetrics_SetsInterruptedAndCompleted()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddMinutes(-15);
        var usage = new RouteUsage(_validUserId, _validRouteId, TransportType.Walking, "mobile", startedAt: startTime);
        var endTime = DateTime.UtcNow;

        // Act
        usage.Interrupt(
            actualDistanceKm: 1.5m,
            actualDurationMin: 12,
            actualCo2Kg: 0.20m,
            endedAt: endTime);

        // Assert
        Assert.True(usage.Interrupted);
        Assert.True(usage.Completed, "Un trayecto interrumpido debe marcar Completed=true para contabilizar en estadísticas.");
        Assert.Equal(1.5m, usage.ActualDistanceKm);
        Assert.Equal(12, usage.ActualDurationMin);
        Assert.Equal(0.20m, usage.ActualCo2Kg);
        Assert.Equal(endTime, usage.EndedAt);
    }

    [Fact]
    public void Interrupt_WithNegativeDistance_ThrowsDomainException()
    {
        // Arrange
        var usage = new RouteUsage(_validUserId, _validRouteId, TransportType.Bike, "web");

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            usage.Interrupt(actualDistanceKm: -0.1m, actualDurationMin: 5, actualCo2Kg: 0.1m, endedAt: DateTime.UtcNow));

        Assert.Equal("La distancia real no puede ser negativa.", ex.Message);
    }

    [Fact]
    public void Interrupt_WithNegativeDuration_ThrowsDomainException()
    {
        // Arrange
        var usage = new RouteUsage(_validUserId, _validRouteId, TransportType.Bike, "web");

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            usage.Interrupt(actualDistanceKm: 2m, actualDurationMin: -1, actualCo2Kg: 0.1m, endedAt: DateTime.UtcNow));

        Assert.Equal("La duración real no puede ser negativa.", ex.Message);
    }

    [Fact]
    public void Interrupt_WithNegativeCo2_ThrowsDomainException()
    {
        // Arrange
        var usage = new RouteUsage(_validUserId, _validRouteId, TransportType.Bike, "web");

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            usage.Interrupt(actualDistanceKm: 2m, actualDurationMin: 10, actualCo2Kg: -0.01m, endedAt: DateTime.UtcNow));

        Assert.Equal("El CO₂ ahorrado no puede ser negativo.", ex.Message);
    }

    [Fact]
    public void Interrupt_WithEndedAtBeforeStartedAt_ThrowsDomainException()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var usage = new RouteUsage(_validUserId, _validRouteId, TransportType.Car, "web", startedAt: startTime);

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            usage.Interrupt(actualDistanceKm: 1m, actualDurationMin: 5, actualCo2Kg: 0m, endedAt: startTime.AddMinutes(-5)));

        Assert.Equal("La fecha de fin no puede ser anterior al inicio.", ex.Message);
    }

    [Fact]
    public void Interrupt_WhenAlreadyEnded_ThrowsDomainException()
    {
        // Arrange: Completar o interrumpir un trayecto previamente
        var usage = new RouteUsage(_validUserId, _validRouteId, TransportType.Bike, "web");
        usage.Interrupt(actualDistanceKm: 1m, actualDurationMin: 5, actualCo2Kg: 0.1m, endedAt: DateTime.UtcNow);

        // Act & Assert: Intentar interrumpir de nuevo lanza excepción (no es idempotente como Complete)
        var ex = Assert.Throws<DomainException>(() =>
            usage.Interrupt(actualDistanceKm: 2m, actualDurationMin: 8, actualCo2Kg: 0.2m, endedAt: DateTime.UtcNow));

        Assert.Equal("El trayecto ya fue finalizado.", ex.Message);
    }

    [Fact]
    public void BehaviorContrast_CompleteIsIdempotent_Whereas_InterruptThrowsWhenAlreadyEnded()
    {
        // Arrange
        var usageComplete = new RouteUsage(_validUserId, _validRouteId, TransportType.Bike, "web");
        usageComplete.Complete(5m, 10, 0.5m, DateTime.UtcNow);

        var usageInterrupt = new RouteUsage(_validUserId, _validRouteId, TransportType.Bike, "web");
        usageInterrupt.Complete(5m, 10, 0.5m, DateTime.UtcNow);

        // Complete segundo llamado: Retorna sin error (Idempotente)
        usageComplete.Complete(10m, 20, 1.0m, DateTime.UtcNow);
        Assert.Equal(5m, usageComplete.ActualDistanceKm);

        // Interrupt llamado sobre trayecto finalizado: Lanza DomainException
        var ex = Assert.Throws<DomainException>(() =>
            usageInterrupt.Interrupt(2m, 5, 0.1m, DateTime.UtcNow));
        Assert.Equal("El trayecto ya fue finalizado.", ex.Message);
    }
}
