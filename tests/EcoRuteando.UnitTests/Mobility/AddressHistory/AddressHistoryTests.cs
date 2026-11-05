using EcoRuteando.Modules.Mobility.Application.AddressHistory;
using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Presentation.Controllers;
using EcoRuteando.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite.Geometries;
using System.Security.Claims;
using Xunit;

namespace EcoRuteando.UnitTests.Mobility.AddressHistoryTests;

public sealed class AddressHistoryTests
{
    [Fact]
    public void AddressHistory_Entity_Constructor_ShouldSetPropertiesProperly()
    {
        var userId = Guid.NewGuid();
        var point = new Point(-75.2818, 2.9275) { SRID = 4326 };
        var entry = new AddressHistory(
            userId,
            "Carrera 5 # 10-20, Neiva",
            "ChIJN1t_tDeuEmsRUsoyG83frY4",
            point,
            "origin");

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(userId, entry.UserId);
        Assert.Equal("Carrera 5 # 10-20, Neiva", entry.SearchText);
        Assert.Equal("ChIJN1t_tDeuEmsRUsoyG83frY4", entry.PlaceId);
        Assert.Equal(point, entry.Location);
        Assert.Equal("origin", entry.SearchType);
    }

    [Fact]
    public void AddressHistory_Entity_WhenUserIdIsEmpty_ShouldThrowDomainException()
    {
        var point = new Point(-75.2818, 2.9275) { SRID = 4326 };
        Assert.Throws<DomainException>(() =>
            new AddressHistory(Guid.Empty, "Neiva", null, point, "search"));
    }

    [Fact]
    public void AddressHistory_Entity_WhenSearchTextIsEmpty_ShouldThrowDomainException()
    {
        var point = new Point(-75.2818, 2.9275) { SRID = 4326 };
        Assert.Throws<DomainException>(() =>
            new AddressHistory(Guid.NewGuid(), "", null, point, "search"));
    }

    [Fact]
    public void AddressHistory_Entity_WhenLocationIsNull_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new AddressHistory(Guid.NewGuid(), "Neiva", null, null!, "search"));
    }

    [Fact]
    public async Task RoutesController_GetAddressHistory_ShouldReturnOk()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var userId = Guid.NewGuid();

        var expectedList = new List<GetUserAddressHistoryResponse>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SearchText = "Parque Santander",
                Latitude = 2.9275m,
                Longitude = -75.2818m,
                SearchType = "search",
                CreatedAt = DateTime.UtcNow
            }
        };

        mockMediator.Setup(m => m.Send(It.Is<GetUserAddressHistoryQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedList);

        var controller = new RoutesController(mockMediator.Object);
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, "User")
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var actionResult = await controller.GetAddressHistory(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returned = Assert.IsAssignableFrom<IReadOnlyList<GetUserAddressHistoryResponse>>(okResult.Value);
        Assert.Single(returned);
    }

    [Fact]
    public async Task RoutesController_SaveAddressHistory_ShouldReturnCreated()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var userId = Guid.NewGuid();
        var newId = Guid.NewGuid();

        mockMediator.Setup(m => m.Send(It.IsAny<SaveAddressHistoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newId);

        var controller = new RoutesController(mockMediator.Object);
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, "User")
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var request = new SaveAddressHistoryApiRequest
        {
            SearchText = "Universidad Surcolombiana",
            Latitude = 2.9416m,
            Longitude = -75.2970m,
            SearchType = "destination"
        };

        // Act
        var actionResult = await controller.SaveAddressHistory(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
    }

    [Fact]
    public async Task RoutesController_GetAddressHistory_WhenUserUnauthenticated_ShouldReturnUnauthorized()
    {
        var mockMediator = new Mock<IMediator>();
        var controller = new RoutesController(mockMediator.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var actionResult = await controller.GetAddressHistory(CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(actionResult);
    }
}
