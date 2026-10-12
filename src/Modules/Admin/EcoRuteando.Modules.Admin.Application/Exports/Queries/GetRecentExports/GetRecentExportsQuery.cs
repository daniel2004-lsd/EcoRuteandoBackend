using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Exports.Queries.GetRecentExports;

public record GetRecentExportsQuery(Guid? UserId = null, int Limit = 20) : IRequest<IReadOnlyList<AdminExportDto>>;
