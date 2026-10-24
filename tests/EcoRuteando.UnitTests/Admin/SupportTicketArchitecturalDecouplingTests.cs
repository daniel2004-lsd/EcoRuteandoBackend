using EcoRuteando.Modules.Admin.Application.Support.Queries.GetAllTickets;
using EcoRuteando.Modules.Admin.Application.Support.Queries.GetTicketById;
using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Users;
using Moq;
using Xunit;

namespace EcoRuteando.UnitTests.Admin;

public class SupportTicketArchitecturalDecouplingTests
{
    [Fact]
    public void ISupportTicketRepository_ShouldNotHaveGetUserDetailsMethod()
    {
        var method = typeof(ISupportTicketRepository).GetMethod("GetUserDetailsAsync");
        Assert.Null(method);
    }

    [Fact]
    public async Task GetTicketByIdQueryHandler_UsesIUserLookupService_InsteadOfRawSql()
    {
        var repoMock = new Mock<ISupportTicketRepository>();
        var userLookupMock = new Mock<IUserLookupService>();

        var userId = Guid.NewGuid();
        var ticket = SupportTicket.Create(userId, "Problema con ruta", priority: 2);

        repoMock
            .Setup(r => r.GetByIdAsync(ticket.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        userLookupMock
            .Setup(u => u.GetUsersByIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(userId)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, UserLookupDto>
            {
                [userId] = new UserLookupDto(userId, "usuario@test.com", "Carlos Perez")
            });

        var handler = new GetTicketByIdQueryHandler(repoMock.Object, userLookupMock.Object);
        var result = await handler.Handle(new GetTicketByIdQuery(ticket.Id, userId, true), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Carlos Perez", result.UserName);
        Assert.Equal("usuario@test.com", result.UserEmail);
        userLookupMock.Verify(u => u.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllTicketsQueryHandler_UsesIUserLookupService_CorrectlyMapsMultipleUsers()
    {
        var repoMock = new Mock<ISupportTicketRepository>();
        var userLookupMock = new Mock<IUserLookupService>();

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        var tickets = new List<SupportTicket>
        {
            SupportTicket.Create(user1, "Ticket 1", priority: 2),
            SupportTicket.Create(user2, "Ticket 2", priority: 2),
            SupportTicket.Create(null, "Ticket Anonimo", priority: 2)
        };

        repoMock
            .Setup(r => r.GetAllAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tickets);

        userLookupMock
            .Setup(u => u.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, UserLookupDto>
            {
                [user1] = new UserLookupDto(user1, "user1@test.com", "User One"),
                [user2] = new UserLookupDto(user2, "user2@test.com", "User Two")
            });

        var handler = new GetAllTicketsQueryHandler(repoMock.Object, userLookupMock.Object);
        var result = await handler.Handle(new GetAllTicketsQuery(), CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal("User One", result[0].UserName);
        Assert.Equal("User Two", result[1].UserName);
        Assert.Null(result[2].UserName);
    }
}
