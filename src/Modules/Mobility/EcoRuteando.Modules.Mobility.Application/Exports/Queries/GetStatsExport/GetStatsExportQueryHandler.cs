using System.Globalization;
using EcoRuteando.Modules.Mobility.Application.Abstractions.Export;
using EcoRuteando.Modules.Mobility.Application.Exports;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using MediatR;

namespace EcoRuteando.Modules.Mobility.Application.Exports.Queries.GetStatsExport;

public sealed class GetStatsExportQueryHandler
    : IRequestHandler<GetStatsExportQuery, ExportFileResponse>
{
    private readonly IRouteUsageRepository _routeUsageRepository;
    private readonly IExportFileService _exportFileService;

    public GetStatsExportQueryHandler(
        IRouteUsageRepository routeUsageRepository,
        IExportFileService exportFileService)
    {
        _routeUsageRepository = routeUsageRepository;
        _exportFileService = exportFileService;
    }

    public async Task<ExportFileResponse> Handle(
        GetStatsExportQuery request,
        CancellationToken cancellationToken)
    {
        var analytics = await _routeUsageRepository.GetAnalyticsAsync(
            request.From,
            request.To,
            cancellationToken);

        var summaryHeaders = new[] { "Métrica", "Valor" };
        var summaryRows = new IReadOnlyList<object>[]
        {
            new object[] { "Rutas consultadas", analytics.RoutesConsulted },
            new object[] { "Usuarios activos", analytics.ActiveUsers },
            new object[] { "CO₂ ahorrado (kg)", analytics.TotalCo2Kg },
            new object[] { "Trayectos completados", analytics.CompletedTrips },
            new object[] { "Distancia promedio (km)", Math.Round(analytics.AverageDistanceKm, 2) }
        };

        var modeHeaders = new[] { "Modo", "Trayectos", "CO₂ (kg)", "Usuarios" };
        var modeRows = analytics.ByTransportMode
            .Select(s => (IReadOnlyList<object>)new object[]
            {
                s.Mode ?? "sin_especificar", s.Trips, s.Co2Kg, s.Users
            })
            .ToList();

        var monthlyHeaders = new[] { "Periodo", "Trayectos", "CO₂ (kg)" };
        var monthlyRows = analytics.Monthly
            .Select(m => (IReadOnlyList<object>)new object[]
            {
                m.Period, m.Trips, m.Co2Kg
            })
            .ToList();

        var dateStamp = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var baseName = $"estadisticas-{dateStamp}";

        return request.Format switch
        {
            ExportFormat.Csv => new ExportFileResponse(
                _exportFileService.BuildCsv(
                    new CsvTable(summaryHeaders, summaryRows),
                    new CsvTable(modeHeaders, modeRows),
                    new CsvTable(monthlyHeaders, monthlyRows)),
                "text/csv; charset=utf-8",
                $"{baseName}.csv"),

            ExportFormat.Json => new ExportFileResponse(
                _exportFileService.BuildJson(new
                {
                    exportadoEl = DateTime.UtcNow,
                    desde = request.From,
                    hasta = request.To,
                    resumen = new
                    {
                        rutasConsultadas = analytics.RoutesConsulted,
                        usuariosActivos = analytics.ActiveUsers,
                        co2AhorradoKg = analytics.TotalCo2Kg,
                        trayectosCompletados = analytics.CompletedTrips,
                        distanciaPromedioKm = Math.Round(analytics.AverageDistanceKm, 2)
                    },
                    porModo = analytics.ByTransportMode.Select(s => new
                    {
                        modo = s.Mode,
                        trayectos = s.Trips,
                        co2Kg = s.Co2Kg,
                        usuarios = s.Users
                    }),
                    mensual = analytics.Monthly.Select(m => new
                    {
                        periodo = m.Period,
                        trayectos = m.Trips,
                        co2Kg = m.Co2Kg
                    })
                }),
                "application/json",
                $"{baseName}.json"),

            ExportFormat.Xlsx => new ExportFileResponse(
                _exportFileService.BuildXlsx(
                    new XlsxSheet("Resumen", summaryHeaders, summaryRows),
                    new XlsxSheet("Por modo", modeHeaders, modeRows),
                    new XlsxSheet("Mensual", monthlyHeaders, monthlyRows)),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{baseName}.xlsx"),

            _ => throw new ArgumentOutOfRangeException(
                nameof(request.Format), request.Format, "Formato de exportación no soportado.")
        };
    }
}