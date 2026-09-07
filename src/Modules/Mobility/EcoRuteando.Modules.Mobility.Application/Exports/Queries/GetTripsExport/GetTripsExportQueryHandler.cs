using System.Globalization;
using EcoRuteando.Modules.Mobility.Application.Abstractions.Export;
using EcoRuteando.Modules.Mobility.Application.Exports;
using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Exports.Queries.GetTripsExport;

public sealed class GetTripsExportQueryHandler
    : IRequestHandler<GetTripsExportQuery, ExportFileResponse>
{
    private readonly IRouteUsageRepository _routeUsageRepository;
    private readonly IExportFileService _exportFileService;

    public GetTripsExportQueryHandler(
        IRouteUsageRepository routeUsageRepository,
        IExportFileService exportFileService)
    {
        _routeUsageRepository = routeUsageRepository;
        _exportFileService = exportFileService;
    }

    public async Task<ExportFileResponse> Handle(
        GetTripsExportQuery request,
        CancellationToken cancellationToken)
    {
        var usages = await _routeUsageRepository.GetByUserAsync(
            request.UserId,
            cancellationToken);

        var headers = new[]
        {
            "ID del trayecto", "Ruta", "Modo de transporte", "Origen",
            "Iniciado", "Finalizado", "Completado",
            "Distancia (km)", "Duración (min)", "CO₂ ahorrado (kg)", "Calorías estimadas"
        };

        var rows = usages
            .Select(u => (IReadOnlyList<object>)BuildRow(u))
            .ToList();

        var dateStamp = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var baseName = $"trayectos-{dateStamp}";

        return request.Format switch
        {
            ExportFormat.Csv => new ExportFileResponse(
                _exportFileService.BuildCsv(new CsvTable(headers, rows)),
                "text/csv; charset=utf-8",
                $"{baseName}.csv"),

            ExportFormat.Json => new ExportFileResponse(
                _exportFileService.BuildJson(new
                {
                    exportadoEl = DateTime.UtcNow,
                    totalTrayectos = usages.Count,
                    trayectos = usages.Select(u => new
                    {
                        usageId = u.Id,
                        routeId = u.RouteId,
                        ruta = u.Route?.Name ?? "Ruta eliminada",
                        modo = u.TransportMode?.ToPgName(),
                        origen = u.Source.ToPgName(),
                        iniciadoEl = u.StartedAt,
                        finalizadoEl = u.EndedAt,
                        completado = u.Completed,
                        distanciaKm = u.ActualDistanceKm,
                        duracionMin = u.ActualDurationMin,
                        co2AhorradoKg = u.ActualCo2Kg,
                        caloriasEstimadas = u.Route?.EstimatedCalories
                    })
                }),
                "application/json",
                $"{baseName}.json"),

            ExportFormat.Xlsx => new ExportFileResponse(
                _exportFileService.BuildXlsx(
                    new XlsxSheet("Historial", headers, rows)),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{baseName}.xlsx"),

            _ => throw new ArgumentOutOfRangeException(
                nameof(request.Format), request.Format, "Formato de exportación no soportado.")
        };
    }

    private static IReadOnlyList<object> BuildRow(RouteUsage usage)
    {
        return new object[]
        {
            usage.Id,
            usage.Route?.Name ?? "Ruta eliminada",
            usage.TransportMode?.ToPgName() ?? "",
            usage.Source.ToPgName(),
            FormatDateTime(usage.StartedAt),
            usage.EndedAt.HasValue ? FormatDateTime(usage.EndedAt.Value) : "",
            usage.Completed ? "Sí" : "No",
            usage.ActualDistanceKm.HasValue ? usage.ActualDistanceKm.Value : (object)"",
            usage.ActualDurationMin.HasValue ? usage.ActualDurationMin.Value : (object)"",
            usage.ActualCo2Kg.HasValue ? usage.ActualCo2Kg.Value : (object)"",
            usage.Route?.EstimatedCalories.HasValue == true
                ? (object)usage.Route!.EstimatedCalories.Value
                : ""
        };
    }

    private static string FormatDateTime(DateTime value)
        => value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}