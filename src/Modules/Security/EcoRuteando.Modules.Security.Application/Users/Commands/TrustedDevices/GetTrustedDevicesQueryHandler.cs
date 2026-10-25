using EcoRuteando.Modules.Security.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.TrustedDevices;

public sealed class GetTrustedDevicesQueryHandler
    : IRequestHandler<GetTrustedDevicesQuery, IReadOnlyList<TrustedDeviceResponse>>
{
    private readonly ITrustedDeviceRepository _trustedDeviceRepository;

    public GetTrustedDevicesQueryHandler(ITrustedDeviceRepository trustedDeviceRepository)
    {
        _trustedDeviceRepository = trustedDeviceRepository;
    }

    public async Task<IReadOnlyList<TrustedDeviceResponse>> Handle(
        GetTrustedDevicesQuery request,
        CancellationToken cancellationToken)
    {
        var devices = await _trustedDeviceRepository.GetActiveByUserIdAsync(
            request.UserId, cancellationToken);

        return devices.Select(d => new TrustedDeviceResponse(
            d.Id,
            d.DeviceName,
            d.CreatedAt,
            d.ExpiresAt,
            d.IsRevoked,
            d.IsActive)).ToList();
    }
}
