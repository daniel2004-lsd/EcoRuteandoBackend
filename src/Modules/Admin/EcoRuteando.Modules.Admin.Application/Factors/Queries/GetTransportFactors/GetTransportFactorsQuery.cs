using EcoRuteando.Modules.Admin.Application.System.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Factors.Queries.GetTransportFactors;

public record GetTransportFactorsQuery(bool OnlyActive = false) : IRequest<IReadOnlyList<TransportFactorDto>>;
