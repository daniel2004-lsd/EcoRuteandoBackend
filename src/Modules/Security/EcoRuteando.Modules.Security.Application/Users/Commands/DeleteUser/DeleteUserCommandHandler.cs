using EcoRuteando.Modules.Security.Application.Abstractions.Logging;
using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;
using EcoRuteando.Shared.Abstractions.Persistence;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.DeleteUser
{
    public sealed class DeleteUserCommandHandler
     : IRequestHandler<DeleteUserCommand>
    {
        private readonly IUserRepository _repository;
        private readonly ISecurityUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public DeleteUserCommandHandler(
            IUserRepository repository,
            ISecurityUnitOfWork unitOfWork,
            IAuditLogService auditLogService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(
                request.UserId,
                cancellationToken);

            if (user is null)
            {
                throw new NotFoundException("El usuario no existe.");
            }

            var deletedUserId = user.Id;

            await _repository.DeleteAsync(
                user,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogService.LogAsync(
                deletedUserId,
                "user.deleted",
                entityName: "users",
                entityId: deletedUserId.ToString(),
                beforeData: $"{{\"email\":\"{user.Email}\",\"role\":\"{user.PrimaryRole?.Name ?? user.PrimaryRoleId?.ToString()}\"}}",
                cancellationToken: cancellationToken);
        }
    }
}
