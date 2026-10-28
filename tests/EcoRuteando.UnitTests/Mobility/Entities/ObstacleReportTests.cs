using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Shared.Exceptions;
using NetTopologySuite.Geometries;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility.Entities;

public sealed class ObstacleReportTests
{
    private readonly Guid _validUserId = Guid.NewGuid();
    private readonly Point _validPoint = new(new Coordinate(-74.08175, 4.60971)) { SRID = 4326 };

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new ObstacleReport(Guid.Empty, "Bache", "Hueco profundo", _validPoint));

        Assert.Equal("El usuario es obligatorio.", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyReportType_ThrowsDomainException(string? reportType)
    {
        var ex = Assert.Throws<DomainException>(() =>
            new ObstacleReport(_validUserId, reportType!, "Hueco profundo", _validPoint));

        Assert.Equal("El tipo de obstáculo es obligatorio.", ex.Message);
    }

    [Fact]
    public void Constructor_WithReportTypeExceeding100Chars_ThrowsDomainException()
    {
        var longType = new string('x', 101);

        var ex = Assert.Throws<DomainException>(() =>
            new ObstacleReport(_validUserId, longType, "Hueco profundo", _validPoint));

        Assert.Equal("El tipo no puede superar 100 caracteres.", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyDescription_ThrowsDomainException(string? description)
    {
        var ex = Assert.Throws<DomainException>(() =>
            new ObstacleReport(_validUserId, "Bache", description!, _validPoint));

        Assert.Equal("La descripción es obligatoria.", ex.Message);
    }

    [Fact]
    public void Constructor_WithDescriptionExceeding2000Chars_ThrowsDomainException()
    {
        var longDescription = new string('d', 2001);

        var ex = Assert.Throws<DomainException>(() =>
            new ObstacleReport(_validUserId, "Bache", longDescription, _validPoint));

        Assert.Equal("La descripción no puede superar 2000 caracteres.", ex.Message);
    }

    [Fact]
    public void Constructor_WithNullLocation_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new ObstacleReport(_validUserId, "Bache", "Hueco", null!));

        Assert.Equal("La ubicación del obstáculo es obligatoria.", ex.Message);
    }

    [Fact]
    public void Constructor_WithoutPhoto_SetsConfidenceScoreTo40()
    {
        var report = new ObstacleReport(_validUserId, "Obras", "Calle cerrada", _validPoint, null, null);

        Assert.Equal(ReportStatus.Pending, report.Status);
        Assert.True(report.IsActive);
        Assert.Equal((short)40, report.ConfidenceScore);
        Assert.NotNull(report.ExpiresAt);
    }

    [Fact]
    public void Constructor_WithPhoto_SetsConfidenceScoreTo50()
    {
        var report = new ObstacleReport(_validUserId, "Obras", "Calle cerrada", _validPoint, "Calle 100", "https://img.com/pic.jpg");

        Assert.Equal((short)50, report.ConfidenceScore);
        Assert.Equal("Calle 100", report.AddressText);
        Assert.Equal("https://img.com/pic.jpg", report.PhotoUrl);
    }

    [Fact]
    public void UpdateConfidenceScore_ClampsScoreBetween0And100()
    {
        var report = new ObstacleReport(_validUserId, "Bache", "Hueco", _validPoint);

        report.UpdateConfidenceScore(150);
        Assert.Equal((short)100, report.ConfidenceScore);

        report.UpdateConfidenceScore(-20);
        Assert.Equal((short)0, report.ConfidenceScore);
    }

    [Fact]
    public void Validate_WithEmptyValidatorId_ThrowsDomainException()
    {
        var report = new ObstacleReport(_validUserId, "Bache", "Hueco", _validPoint);

        var ex = Assert.Throws<DomainException>(() =>
            report.Validate(Guid.Empty, ReportStatus.Validated));

        Assert.Equal("El validador es obligatorio.", ex.Message);
    }

    [Fact]
    public void Validate_WhenValidatorIsCreator_ThrowsDomainException()
    {
        var report = new ObstacleReport(_validUserId, "Bache", "Hueco", _validPoint);

        var ex = Assert.Throws<DomainException>(() =>
            report.Validate(_validUserId, ReportStatus.Validated));

        Assert.Equal("Un usuario no puede validar su propio reporte.", ex.Message);
    }

    [Theory]
    [InlineData(ReportStatus.Pending)]
    [InlineData(ReportStatus.UnderReview)]
    public void Validate_WithInvalidTargetStatus_ThrowsDomainException(ReportStatus invalidStatus)
    {
        var validatorId = Guid.NewGuid();
        var report = new ObstacleReport(_validUserId, "Bache", "Hueco", _validPoint);

        var ex = Assert.Throws<DomainException>(() =>
            report.Validate(validatorId, invalidStatus));

        Assert.Equal("El reporte solo puede quedar validado o rechazado.", ex.Message);
    }

    [Fact]
    public void Validate_WithValidData_UpdatesStatusAndValidator()
    {
        var validatorId = Guid.NewGuid();
        var report = new ObstacleReport(_validUserId, "Bache", "Hueco", _validPoint);

        report.Validate(validatorId, ReportStatus.Validated, "Verificado en terreno");

        Assert.Equal(ReportStatus.Validated, report.Status);
        Assert.Equal(validatorId, report.ValidatorId);
        Assert.Equal("Verificado en terreno", report.ValidationNote);
        Assert.NotNull(report.ValidatedAt);
    }

    [Fact]
    public void Deactivate_And_Reactivate_ChangesIsActiveState()
    {
        var report = new ObstacleReport(_validUserId, "Bache", "Hueco", _validPoint);

        report.Deactivate();
        Assert.False(report.IsActive);

        var newExpiration = DateTime.UtcNow.AddHours(48);
        report.Reactivate(newExpiration);
        Assert.True(report.IsActive);
        Assert.Equal(newExpiration, report.ExpiresAt);
    }
}
