using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EcoRuteando.Shared.Abstractions;
using System.Threading.Tasks;
using EcoRuteando.Shared.Abstractions.Persistence;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.DeleteUser
{
    public sealed class DeleteUserCommandHandler
     : IRequestHandler<DeleteUserCommand>
    {
        private readonly IUserRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteUserCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
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

            await _repository.DeleteAsync(
                user,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    } 
}
