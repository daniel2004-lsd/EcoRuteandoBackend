using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Exports.Queries.GetExportById;

public record GetExportByIdQuery(Guid Id) : IRequest<AdminExportDto?>;
