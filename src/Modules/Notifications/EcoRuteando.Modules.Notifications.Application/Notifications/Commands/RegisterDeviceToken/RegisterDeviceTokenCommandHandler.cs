using EcoRuteando.Modules.Notifications.Domain.Entities;
using EcoRuteando.Modules.Notifications.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Notifications.Application.Notifications.Commands.RegisterDeviceToken;

public class RegisterDeviceTokenCommandHandler : IRequestHandler<RegisterDeviceTokenCommand, bool>
{
    private readonly IDeviceTokenRepository _repository;

    public RegisterDeviceTokenCommandHandler(IDeviceTokenRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(RegisterDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByUserAndPlatformAsync(request.UserId, request.Platform, cancellationToken);
        if (existing != null)
        {
            existing.UpdateToken(request.ExpoPushToken, request.DeviceId);
        }
        else
        {
            var token = DeviceToken.Create(request.UserId, request.Platform, request.ExpoPushToken, request.DeviceId);
            await _repository.AddAsync(token, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
