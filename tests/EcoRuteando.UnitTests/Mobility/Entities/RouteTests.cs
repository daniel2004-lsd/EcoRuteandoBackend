using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Shared.Exceptions;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility.Entities;

public sealed class RouteTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyName_ThrowsDomainException(string? name)
    {
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            new Route(name!, TransportType.Bike, "Origen", "Destino"));

        Assert.Equal("El nombre de la ruta es obligatorio.", ex.Message);
    }

    [Fact]
    public void Constructor_WithNameExceeding150Chars_ThrowsDomainException()
    {
        // Arrange
        var longName = new string('A', 151);

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            new Route(longName, TransportType.Bike, "Origen", "Destino"));

        Assert.Equal("El nombre de la ruta no puede superar 150 caracteres.", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyStartName_ThrowsDomainException(string? startName)
    {
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            new Route("Ruta Válida", TransportType.Bike, startName!, "Destino"));

        Assert.Equal("El punto de origen es obligatorio.", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyDestinationName_ThrowsDomainException(string? destName)
    {
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            new Route("Ruta Válida", TransportType.Bike, "Origen", destName!));

        Assert.Equal("El punto de destino es obligatorio.", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Constructor_WithInvalidDifficultyLevel_ThrowsDomainException(short difficulty)
    {
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            new Route("Ruta Dificultad", TransportType.Bike, "Origen", "Destino", difficultyLevel: difficulty));

        Assert.Equal("El nivel de dificultad debe estar entre 1 y 5.", ex.Message);
    }

    [Fact]
    public void Constructor_WithNegativeDistance_ThrowsDomainException()
    {
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            new Route("Ruta Distancia", TransportType.Bike, "Origen", "Destino", distanceKm: -0.5m));

        Assert.Equal("La distancia no puede ser negativa.", ex.Message);
    }

    [Fact]
    public void Constructor_WithNegativeEstimatedTime_ThrowsDomainException()
    {
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() =>
            new Route("Ruta Tiempo", TransportType.Bike, "Origen", "Destino", estimatedTimeMin: -10));

        Assert.Equal("El tiempo estimado no puede ser negativo.", ex.Message);
    }

    [Fact]
    public void Constructor_WithValidArguments_InitializesRouteCorrectly()
    {
        // Act
        var route = new Route(
            name: " Ruta Ciclovía Norte ",
            transportType: TransportType.Bike,
            startName: " Parque Santander ",
            destinationName: " Sena Industria ",
            description: " Recorrido ecológico matutino ",
            distanceKm: 8.4m,
            estimatedTimeMin: 25,
            co2SavedKg: 1.25m,
            difficultyLevel: 3);

        // Assert
        Assert.NotEqual(Guid.Empty, route.Id);
        Assert.Equal("Ruta Ciclovía Norte", route.Name);
        Assert.Equal("Parque Santander", route.StartName);
        Assert.Equal("Sena Industria", route.DestinationName);
        Assert.Equal("Recorrido ecológico matutino", route.Description);
        Assert.Equal(TransportType.Bike, route.TransportType);
        Assert.Equal(RouteStatus.Active, route.Status);
        Assert.Equal(8.4m, route.DistanceKm);
        Assert.Equal(25, route.EstimatedTimeMin);
        Assert.Equal(1.25m, route.Co2SavedKg);
        Assert.Equal((short)3, route.DifficultyLevel);
        Assert.Empty(route.RoutePois);
    }

    [Fact]
    public void AddPoi_WithEmptyPoiId_ThrowsDomainException()
    {
        // Arrange
        var route = new Route("Ruta POI", TransportType.Walking, "Inicio", "Fin");

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => route.AddPoi(Guid.Empty));
        Assert.Equal("El punto de interés es obligatorio.", ex.Message);
    }

    [Fact]
    public void AddPoi_WithValidPoiId_AddsPoiToCollection()
    {
        // Arrange
        var route = new Route("Ruta POI", TransportType.Walking, "Inicio", "Fin");
        var poiId = Guid.NewGuid();

        // Act
        route.AddPoi(poiId, sortOrder: 1);

        // Assert
        Assert.Single(route.RoutePois);
        var added = route.RoutePois.First();
        Assert.Equal(poiId, added.PoiId);
        Assert.Equal(route.Id, added.RouteId);
        Assert.Equal((short)1, added.SortOrder);
    }

    [Fact]
    public void AddPoi_DuplicatePoi_ThrowsDomainException()
    {
        // Arrange
        var route = new Route("Ruta POI", TransportType.Walking, "Inicio", "Fin");
        var poiId = Guid.NewGuid();
        route.AddPoi(poiId);

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => route.AddPoi(poiId));
        Assert.Equal("El punto de interés ya está asociado a la ruta.", ex.Message);
    }
}
