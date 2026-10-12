using EcoRuteando.Modules.Admin.Application.System.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Factors.Queries.GetTransportFactors;

public class GetTransportFactorsQueryHandler : IRequestHandler<GetTransportFactorsQuery, IReadOnlyList<TransportFactorDto>>
{
    private readonly ITransportFactorRepository _repository;

    public GetTransportFactorsQueryHandler(ITransportFactorRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<TransportFactorDto>> Handle(GetTransportFactorsQuery request, CancellationToken cancellationToken)
    {
        var factors = request.OnlyActive
            ? await _repository.GetActiveAsync(cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);

        var now = DateTime.UtcNow;

        return factors.Select(f => new TransportFactorDto(
            f.Id,
            f.TransportType,
            f.TransportType.ToString(),
            f.Co2FactorKgKm,
            f.CalorieFactorKm,
            f.ValidFrom,
            f.ValidUntil,
            f.Source,
            f.CreatedAt,
            f.ValidFrom <= now && (!f.ValidUntil.HasValue || f.ValidUntil.Value > now)
        )).ToList();
    }
}
