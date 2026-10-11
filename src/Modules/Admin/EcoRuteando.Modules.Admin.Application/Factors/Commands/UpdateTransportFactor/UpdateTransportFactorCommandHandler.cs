using EcoRuteando.Modules.Admin.Application.System.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Factors.Commands.UpdateTransportFactor;

public class UpdateTransportFactorCommandHandler : IRequestHandler<UpdateTransportFactorCommand, TransportFactorDto>
{
    private readonly ITransportFactorRepository _repository;

    public UpdateTransportFactorCommandHandler(ITransportFactorRepository repository)
    {
        _repository = repository;
    }

    public async Task<TransportFactorDto> Handle(UpdateTransportFactorCommand request, CancellationToken cancellationToken)
    {
        var factor = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (factor == null)
            throw new KeyNotFoundException($"Transport factor with ID '{request.Id}' was not found.");

        factor.Update(
            request.Co2FactorKgKm,
            request.CalorieFactorKm,
            request.ValidFrom,
            request.ValidUntil,
            request.Source);

        await _repository.UpdateAsync(factor, cancellationToken);

        var now = DateTime.UtcNow;
        return new TransportFactorDto(
            factor.Id,
            factor.TransportType,
            factor.TransportType.ToString(),
            factor.Co2FactorKgKm,
            factor.CalorieFactorKm,
            factor.ValidFrom,
            factor.ValidUntil,
            factor.Source,
            factor.CreatedAt,
            factor.ValidFrom <= now && (!factor.ValidUntil.HasValue || factor.ValidUntil.Value > now));
    }
}
