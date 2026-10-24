using EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.DeactivatePoi;
using EcoRuteando.Modules.Mobility.Application.PointsOfInterest.Commands.UpdatePoi;
using EcoRuteando.Modules.Mobility.Application.RoutePois.Commands.AddPoiToRoute;
using EcoRuteando.Modules.Mobility.Application.RoutePois.Commands.RemovePoiFromRoute;
using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using Moq;
using NetTopologySuite.Geometries;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility;

public class PoiOwnershipSecurityTests
{
    private readonly Mock<IPointOfInterestRepository> _poiRepoMock = new();
    private readonly Mock<IRouteRepository> _routeRepoMock = new();
    private readonly Mock<IMobilityUnitOfWork> _uowMock = new();

    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _attackerId = Guid.NewGuid();
    private readonly Guid _adminId = Guid.NewGuid();

    private PointOfInterest CreateTestPoi(Guid? createdBy)
    {
        return new PointOfInterest(
            name: "Parque Simón Bolívar",
            poiType: "Parque",
            location: new Point(-74.08, 4.65),
            createdBy: createdBy);
    }

    private Route CreateTestRoute(Guid? createdBy)
    {
        return new Route(
            name: "Ruta Trabajo",
            transportType: TransportType.Bike,
            startName: "Casa",
            destinationName: "Oficina",
            createdBy: createdBy);
    }

    // ── UpdatePoi Tests ───────────────────────────────────────────

    [Fact]
    public async Task UpdatePoi_WhenUserIsOwner_ShouldSucceed()
    {
        var poi = CreateTestPoi(_ownerId);
        _poiRepoMock.Setup(r => r.GetByIdAsync(poi.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poi);

        var handler = new UpdatePoiCommandHandler(_poiRepoMock.Object, _uowMock.Object);
        var command = new UpdatePoiCommand(
            Id: poi.Id,
            Name: "Parque Actualizado",
            PoiType: "Parque",
            Lat: 4.66,
            Lng: -74.09,
            RequestedByUserId: _ownerId,
            IsAdmin: false);

        await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Parque Actualizado", poi.Name);
        _poiRepoMock.Verify(r => r.Update(poi), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePoi_WhenUserIsNotOwnerAndNotAdmin_ShouldThrowForbiddenException()
    {
        var poi = CreateTestPoi(_ownerId);
        _poiRepoMock.Setup(r => r.GetByIdAsync(poi.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poi);

        var handler = new UpdatePoiCommandHandler(_poiRepoMock.Object, _uowMock.Object);
        var command = new UpdatePoiCommand(
            Id: poi.Id,
            Name: "Hackeado",
            PoiType: "Parque",
            RequestedByUserId: _attackerId,
            IsAdmin: false);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task UpdatePoi_WhenUserIsAdmin_ShouldSucceedEvenIfNotOwner()
    {
        var poi = CreateTestPoi(_ownerId);
        _poiRepoMock.Setup(r => r.GetByIdAsync(poi.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poi);

        var handler = new UpdatePoiCommandHandler(_poiRepoMock.Object, _uowMock.Object);
        var command = new UpdatePoiCommand(
            Id: poi.Id,
            Name: "Admin Update",
            PoiType: "Parque",
            Lat: 4.66,
            Lng: -74.09,
            RequestedByUserId: _adminId,
            IsAdmin: true);

        await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Admin Update", poi.Name);
        _poiRepoMock.Verify(r => r.Update(poi), Times.Once);
    }

    [Fact]
    public async Task UpdatePoi_WhenPoiNotFound_ShouldThrowNotFoundException()
    {
        _poiRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PointOfInterest?)null);

        var handler = new UpdatePoiCommandHandler(_poiRepoMock.Object, _uowMock.Object);
        var command = new UpdatePoiCommand(
            Id: Guid.NewGuid(),
            Name: "Test",
            PoiType: "Parque",
            RequestedByUserId: _ownerId,
            IsAdmin: false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    // ── DeactivatePoi Tests ─────────────────────────────────────────

    [Fact]
    public async Task DeactivatePoi_WhenUserIsOwner_ShouldSucceed()
    {
        var poi = CreateTestPoi(_ownerId);
        _poiRepoMock.Setup(r => r.GetByIdAsync(poi.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poi);

        var handler = new DeactivatePoiCommandHandler(_poiRepoMock.Object, _uowMock.Object);
        var command = new DeactivatePoiCommand(poi.Id, RequestedByUserId: _ownerId, IsAdmin: false);

        await handler.Handle(command, CancellationToken.None);

        Assert.False(poi.IsActive);
        _poiRepoMock.Verify(r => r.Update(poi), Times.Once);
    }

    [Fact]
    public async Task DeactivatePoi_WhenUserIsNotOwnerAndNotAdmin_ShouldThrowForbiddenException()
    {
        var poi = CreateTestPoi(_ownerId);
        _poiRepoMock.Setup(r => r.GetByIdAsync(poi.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poi);

        var handler = new DeactivatePoiCommandHandler(_poiRepoMock.Object, _uowMock.Object);
        var command = new DeactivatePoiCommand(poi.Id, RequestedByUserId: _attackerId, IsAdmin: false);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    // ── RoutePoi Association Tests ──────────────────────────────────

    [Fact]
    public async Task AddPoiToRoute_WhenUserIsRouteOwner_ShouldSucceed()
    {
        var route = CreateTestRoute(_ownerId);
        var poi = CreateTestPoi(_ownerId);

        _routeRepoMock.Setup(r => r.GetWithPoisByIdAsync(route.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(route);
        _poiRepoMock.Setup(r => r.GetByIdAsync(poi.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poi);

        var handler = new AddPoiToRouteCommandHandler(_routeRepoMock.Object, _poiRepoMock.Object, _uowMock.Object);
        var command = new AddPoiToRouteCommand(route.Id, poi.Id, 1, RequestedByUserId: _ownerId, IsAdmin: false);

        await handler.Handle(command, CancellationToken.None);

        Assert.Single(route.RoutePois);
        Assert.Equal(poi.Id, route.RoutePois.First().PoiId);
    }

    [Fact]
    public async Task AddPoiToRoute_WhenUserIsNotRouteOwner_ShouldThrowForbiddenException()
    {
        var route = CreateTestRoute(_ownerId);
        var poi = CreateTestPoi(_attackerId);

        _routeRepoMock.Setup(r => r.GetWithPoisByIdAsync(route.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(route);

        var handler = new AddPoiToRouteCommandHandler(_routeRepoMock.Object, _poiRepoMock.Object, _uowMock.Object);
        var command = new AddPoiToRouteCommand(route.Id, poi.Id, 1, RequestedByUserId: _attackerId, IsAdmin: false);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task AddPoiToRoute_WhenAttachingAnotherUsersPrivatePoi_ShouldThrowForbiddenException()
    {
        var route = CreateTestRoute(_ownerId);
        var foreignPrivatePoi = CreateTestPoi(_attackerId);

        _routeRepoMock.Setup(r => r.GetWithPoisByIdAsync(route.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(route);
        _poiRepoMock.Setup(r => r.GetByIdAsync(foreignPrivatePoi.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(foreignPrivatePoi);

        var handler = new AddPoiToRouteCommandHandler(_routeRepoMock.Object, _poiRepoMock.Object, _uowMock.Object);
        var command = new AddPoiToRouteCommand(route.Id, foreignPrivatePoi.Id, 1, RequestedByUserId: _ownerId, IsAdmin: false);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task RemovePoiFromRoute_WhenUserIsNotRouteOwner_ShouldThrowForbiddenException()
    {
        var route = CreateTestRoute(_ownerId);
        var poiId = Guid.NewGuid();
        route.AddPoi(poiId, 1);

        _routeRepoMock.Setup(r => r.GetWithPoisByIdAsync(route.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(route);

        var handler = new RemovePoiFromRouteCommandHandler(_routeRepoMock.Object, _uowMock.Object);
        var command = new RemovePoiFromRouteCommand(route.Id, poiId, RequestedByUserId: _attackerId, IsAdmin: false);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
