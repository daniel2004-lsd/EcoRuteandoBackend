using EcoRuteando.Modules.Security.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.TrustedDevices;

public sealed class RevokeTrustedDeviceCommandHandler
    : IRequestHandler<RevokeTrustedDeviceCommand, Unit>
{
    private readonly ITrustedDeviceRepository _trustedDeviceRepository;
    private readonly ISecurityUnitOfWork _unitOfWork;

    public RevokeTrustedDeviceCommandHandler(
        ITrustedDeviceRepository trustedDeviceRepository,
        ISecurityUnitOfWork unitOfWork)
    {
        _trustedDeviceRepository = trustedDeviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        RevokeTrustedDeviceCommand request,
        CancellationToken cancellationToken)
    {
        var device = await _trustedDeviceRepository.GetByIdAsync(
            request.DeviceId, cancellationToken);

        if (device is null || device.UserId != request.UserId)
        {
            throw new NotFoundException("Dispositivo de confianza no encontrado.");
        }

        if (!device.IsRevoked)
        {
            device.Revoke();
            _trustedDeviceRepository.Update(device);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
