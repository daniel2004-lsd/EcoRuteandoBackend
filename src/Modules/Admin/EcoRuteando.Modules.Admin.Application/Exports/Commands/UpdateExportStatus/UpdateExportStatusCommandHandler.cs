using EcoRuteando.Modules.Admin.Application.Analytics.Dtos;
using EcoRuteando.Modules.Admin.Domain.Enums;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Admin.Application.Exports.Commands.UpdateExportStatus;

public class UpdateExportStatusCommandHandler : IRequestHandler<UpdateExportStatusCommand, AdminExportDto>
{
    private readonly IAdminExportRepository _repository;

    public UpdateExportStatusCommandHandler(IAdminExportRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminExportDto> Handle(UpdateExportStatusCommand request, CancellationToken cancellationToken)
    {
        var export = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (export == null)
            throw new KeyNotFoundException($"Export with ID '{request.Id}' was not found.");

        if (request.Status == ExportStatus.Ready)
        {
            export.Complete(request.FileUrl ?? string.Empty, request.SizeBytes ?? 0, request.RecordsCount ?? 0);
        }
        else if (request.Status == ExportStatus.Error)
        {
            export.Fail(request.ErrorMessage ?? "Export failed.");
        }

        await _repository.UpdateAsync(export, cancellationToken);

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
