using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Exports.Commands.RequestExport;

public class RequestExportCommandHandler : IRequestHandler<RequestExportCommand, AdminExportDto>
{
    private readonly IAdminExportRepository _repository;

    public RequestExportCommandHandler(IAdminExportRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminExportDto> Handle(RequestExportCommand request, CancellationToken cancellationToken)
    {
        var export = AdminExport.Create(
            request.UserId,
            request.ExportType,
            request.Format,
            request.Filters);

        await _repository.AddAsync(export, cancellationToken);

        return new AdminExportDto(
            export.Id,
            export.UserId,
            export.ExportType,
            export.Format,
            export.Format.ToString(),
            export.Filters,
            export.FileUrl,
            export.Status,
            export.Status.ToString(),
            export.ErrorMessage,
            export.SizeBytes,
            export.RecordsCount,
            export.CreatedAt,
            export.ExpiresAt);
    }
}
