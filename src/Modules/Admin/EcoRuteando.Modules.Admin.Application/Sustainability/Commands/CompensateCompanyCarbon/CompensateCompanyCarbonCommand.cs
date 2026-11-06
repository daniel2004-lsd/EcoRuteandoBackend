using EcoRuteando.Modules.Admin.Application.Sustainability.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Sustainability.Commands.CompensateCompanyCarbon;

public sealed record CompensateCompanyCarbonCommand(
    Guid CompanyId,
    decimal Co2CompensatedKg,
    string? PartnerProgram = null,
    string? Notes = null) : IRequest<CompensateCompanyCarbonResponse>;
