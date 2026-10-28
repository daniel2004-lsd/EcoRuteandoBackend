using EcoRuteando.Modules.Admin.Application.Support.Commands.AddTicketMessage;
using EcoRuteando.Modules.Admin.Application.Support.Queries.GetTicketById;
using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EcoRuteando.UnitTests.Admin.Support;

public class SupportTicketSecurityTests
{
    private readonly Mock<ISupportTicketRepository> _repositoryMock = new();
    private readonly Mock<IUserLookupService> _userLookupMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<ILogger<AddTicketMessageCommandHandler>> _loggerMock = new();

    public SupportTicketSecurityTests()
    {
        _userLookupMock
            .Setup(u => u.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, UserLookupDto>());
    }

    [Fact]
    public async Task GetTicketById_WhenRequestingUserIsOwner_ShouldReturnTicket()
    {
        var ownerId = Guid.NewGuid();
        var ticket = SupportTicket.Create(ownerId, "Test ticket");
        _repositoryMock
            .Setup(r => r.GetByIdAsync(ticket.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var handler = new GetTicketByIdQueryHandler(_repositoryMock.Object, _userLookupMock.Object);
        var query = new GetTicketByIdQuery(ticket.Id, ownerId, IsAdmin: false);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
    }

    [Fact]
    public async Task GetTicketById_WhenRequestingUserIsAdmin_ShouldReturnTicketRegardlessOfOwner()
    {
        var ownerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var ticket = SupportTicket.Create(ownerId, "Test ticket");
        _repositoryMock
            .Setup(r => r.GetByIdAsync(ticket.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var handler = new GetTicketByIdQueryHandler(_repositoryMock.Object, _userLookupMock.Object);
        var query = new GetTicketByIdQuery(ticket.Id, adminId, IsAdmin: true);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
    }

    [Fact]
    public async Task GetTicketById_WhenRequestingUserIsNotOwnerAndNotAdmin_ShouldReturnNullToPreventIdor()
    {
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var ticket = SupportTicket.Create(ownerId, "Private issue");
        _repositoryMock
            .Setup(r => r.GetByIdAsync(ticket.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var handler = new GetTicketByIdQueryHandler(_repositoryMock.Object, _userLookupMock.Object);
        var query = new GetTicketByIdQuery(ticket.Id, attackerId, IsAdmin: false);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddTicketMessage_WhenSenderIsNotOwnerAndNotAdmin_ShouldReturnNullToPreventIdor()
    {
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var ticket = SupportTicket.Create(ownerId, "Victim ticket");
        _repositoryMock
            .Setup(r => r.GetByIdAsync(ticket.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var handler = new AddTicketMessageCommandHandler(
            _repositoryMock.Object,
            _userLookupMock.Object,
            _configMock.Object,
            _loggerMock.Object);

        var command = new AddTicketMessageCommand(
            ticket.Id,
            attackerId,
            IsAdmin: false,
            Message: "Injected malicious message",
            AttachmentUrl: null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Null(result);
        _repositoryMock.Verify(r => r.AddMessageAsync(It.IsAny<SupportMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddTicketMessage_WhenSenderIsOwner_ShouldAddMessage()
    {
        var ownerId = Guid.NewGuid();
        var ticket = SupportTicket.Create(ownerId, "My ticket");
        _repositoryMock
            .Setup(r => r.GetByIdAsync(ticket.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var handler = new AddTicketMessageCommandHandler(
            _repositoryMock.Object,
            _userLookupMock.Object,
            _configMock.Object,
            _loggerMock.Object);

        var command = new AddTicketMessageCommand(
            ticket.Id,
            ownerId,
            IsAdmin: false,
            Message: "Clarification from owner",
            AttachmentUrl: null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        _repositoryMock.Verify(r => r.AddMessageAsync(It.IsAny<SupportMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
