using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.UpdateMyPhoto;

public sealed class UpdateMyPhotoCommandHandler
    : IRequestHandler<UpdateMyPhotoCommand, UpdateMyPhotoResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;

    // 5 MB máximo en base64 (~3.7 MB de imagen real)
    private const int MaxBase64Length = 5 * 1024 * 1024;

    public UpdateMyPhotoCommandHandler(
        IUserRepository userRepository,
        ISecurityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateMyPhotoResponse> Handle(
        UpdateMyPhotoCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        if (!string.IsNullOrWhiteSpace(request.PhotoBase64))
        {
            if (request.PhotoBase64.Length > MaxBase64Length)
            {
                throw new DomainException(
                    "La imagen es demasiado grande. Máximo 5 MB.");
            }

            if (!request.PhotoBase64.StartsWith("data:image/"))
            {
                throw new DomainException(
                    "Formato de imagen no válido. Usa un data URI base64.");
            }
        }

        user.SetProfilePhoto(
            string.IsNullOrWhiteSpace(request.PhotoBase64)
                ? null
                : request.PhotoBase64);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateMyPhotoResponse(user.Id, user.ProfilePhotoUrl);
    }
}
