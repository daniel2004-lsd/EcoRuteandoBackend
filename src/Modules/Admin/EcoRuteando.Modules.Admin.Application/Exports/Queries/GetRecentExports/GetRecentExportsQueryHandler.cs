using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Exports.Queries.GetRecentExports;

public class GetRecentExportsQueryHandler : IRequestHandler<GetRecentExportsQuery, IReadOnlyList<AdminExportDto>>
{
    private readonly IAdminExportRepository _repository;

    public GetRecentExportsQueryHandler(IAdminExportRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AdminExportDto>> Handle(GetRecentExportsQuery request, CancellationToken cancellationToken)
    {
        var exports = await _repository.GetRecentAsync(request.UserId, request.Limit, cancellationToken);

        return exports.Select(e => new AdminExportDto(
            e.Id,
            e.UserId,
            e.ExportType,
            e.Format,
            e.Format.ToString(),
            e.Filters,
            e.FileUrl,
            e.Status,
            e.Status.ToString(),
            e.ErrorMessage,
            e.SizeBytes,
            e.RecordsCount,
            e.CreatedAt,
            e.ExpiresAt
        )).ToList();
    }
}
