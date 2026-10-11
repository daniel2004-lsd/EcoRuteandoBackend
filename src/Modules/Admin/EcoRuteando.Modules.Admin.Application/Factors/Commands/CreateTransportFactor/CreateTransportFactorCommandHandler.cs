using EcoRuteando.Modules.Admin.Application.System.Dtos;
using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Factors.Commands.CreateTransportFactor;

public class CreateTransportFactorCommandHandler : IRequestHandler<CreateTransportFactorCommand, TransportFactorDto>
{
    private readonly ITransportFactorRepository _repository;

    public CreateTransportFactorCommandHandler(ITransportFactorRepository repository)
    {
        _repository = repository;
    }

    public async Task<TransportFactorDto> Handle(CreateTransportFactorCommand request, CancellationToken cancellationToken)
    {
        var validFrom = request.ValidFrom ?? DateTime.UtcNow;

        var existingActive = await _repository.GetActiveByTransportTypeAsync(request.TransportType, cancellationToken);
        if (existingActive != null && (!existingActive.ValidUntil.HasValue || existingActive.ValidUntil.Value > validFrom))
        {
            existingActive.Expire(validFrom);
            await _repository.UpdateAsync(existingActive, cancellationToken);
        }

        var factor = TransportFactor.Create(
            request.TransportType,
            request.Co2FactorKgKm,
            request.CalorieFactorKm,
            validFrom,
            request.ValidUntil,
            request.Source);

        await _repository.AddAsync(factor, cancellationToken);

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
