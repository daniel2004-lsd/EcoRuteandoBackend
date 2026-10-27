using EcoRuteando.Modules.Mobility.Application.Routes.Queries.GetRouteById;
using EcoRuteando.Modules.Mobility.Application.Routes.Queries.GetRoutes;
using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using Moq;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility;

public class RoutePrivacyAndPaginationTests
{
    private readonly Mock<IRouteRepository> _repoMock;

    public RoutePrivacyAndPaginationTests()
    {
        _repoMock = new Mock<IRouteRepository>();
    }

    [Fact]
    public async Task GetRoutesQueryHandler_PassesUserIsolationAndPaginationToRepository()
    {
        var userId = Guid.NewGuid();
        var handler = new GetRoutesQueryHandler(_repoMock.Object);

        _repoMock
            .Setup(r => r.GetRoutesAsync(
                TransportType.Bike,
                false,
                userId,
                2,
                25,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Route>());

        var query = new GetRoutesQuery(
            TransportType: TransportType.Bike,
            IncludeInactive: false,
            CreatedBy: userId,
            Page: 2,
            PageSize: 25);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);

        _repoMock.Verify(r => r.GetRoutesAsync(
            TransportType.Bike,
            false,
            userId,
            2,
            25,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRouteByIdQueryHandler_ActiveRoute_CanBeViewedByAnyUser()
    {
        var routeId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var route = new Route(
            name: "Ruta Publica",
            transportType: TransportType.Walking,
            startName: "Inicio",
            destinationName: "Fin",
            createdBy: authorId);

        _repoMock
            .Setup(r => r.GetWithPoisByIdAsync(routeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(route);

        var handler = new GetRouteByIdQueryHandler(_repoMock.Object);
        var result = await handler.Handle(new GetRouteByIdQuery(routeId, RequestedByUserId: requesterId, IsAdmin: false), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Ruta Publica", result.Name);
    }

    [Fact]
    public async Task GetRouteByIdQueryHandler_InactiveRoute_ViewedByAuthor_Allowed()
    {
        var routeId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var route = new Route(
            name: "Ruta Borrador",
            transportType: TransportType.Bike,
            startName: "Inicio",
            destinationName: "Fin",
            createdBy: authorId);

        route.Deactivate();

        _repoMock
            .Setup(r => r.GetWithPoisByIdAsync(routeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(route);

        var handler = new GetRouteByIdQueryHandler(_repoMock.Object);
        var result = await handler.Handle(new GetRouteByIdQuery(routeId, RequestedByUserId: authorId, IsAdmin: false), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Ruta Borrador", result.Name);
    }

    [Fact]
    public async Task GetRouteByIdQueryHandler_InactiveRoute_ViewedByAdmin_Allowed()
    {
        var routeId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var route = new Route(
            name: "Ruta Inactiva",
            transportType: TransportType.Bike,
            startName: "Inicio",
            destinationName: "Fin",
            createdBy: authorId);

        route.Deactivate();

        _repoMock
            .Setup(r => r.GetWithPoisByIdAsync(routeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(route);

        var handler = new GetRouteByIdQueryHandler(_repoMock.Object);
        var result = await handler.Handle(new GetRouteByIdQuery(routeId, RequestedByUserId: adminId, IsAdmin: true), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Ruta Inactiva", result.Name);
    }

    [Fact]
    public async Task GetRouteByIdQueryHandler_InactiveRoute_ViewedByOtherUser_ThrowsNotFound()
    {
        var routeId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var route = new Route(
            name: "Ruta Inactiva Privada",
            transportType: TransportType.Bike,
            startName: "Inicio",
            destinationName: "Fin",
            createdBy: authorId);

        route.Deactivate();

        _repoMock
            .Setup(r => r.GetWithPoisByIdAsync(routeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(route);

        var handler = new GetRouteByIdQueryHandler(_repoMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetRouteByIdQuery(routeId, RequestedByUserId: otherUserId, IsAdmin: false), CancellationToken.None));
    }
}
