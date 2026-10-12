using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Exports.Queries.GetExportById;

public class GetExportByIdQueryHandler : IRequestHandler<GetExportByIdQuery, AdminExportDto?>
{
    private readonly IAdminExportRepository _repository;

    public GetExportByIdQueryHandler(IAdminExportRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminExportDto?> Handle(GetExportByIdQuery request, CancellationToken cancellationToken)
    {
        var e = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (e == null)
            return null;

        return new AdminExportDto(
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
            e.ExpiresAt);
    }
}
