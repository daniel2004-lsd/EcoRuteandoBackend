using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.Sessions;

public sealed class RevokeAllSessionsCommandHandler
    : IRequestHandler<RevokeAllSessionsCommand, int>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public RevokeAllSessionsCommandHandler(
        ISessionRepository sessionRepository,
        ISecurityUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(
        RevokeAllSessionsCommand request,
        CancellationToken cancellationToken)
    {
        var sessions = await _sessionRepository.GetActiveByUserIdAsync(
            request.UserId, cancellationToken);

        var count = 0;
        foreach (var session in sessions)
        {
            session.Revoke();
            _sessionRepository.Update(session);
            count++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return count;
    }
}
