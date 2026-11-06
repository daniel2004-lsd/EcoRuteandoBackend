using System.Text.Json;
using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using EcoRuteando.Shared.Abstractions.Persistence;
using EcoRuteando.Shared.Exceptions;
using MediatR;
using NetTopologySuite.Geometries;

namespace EcoRuteando.Modules.Mobility.Application.TripHistory.Commands.CompleteTrip;

public sealed class CompleteTripCommandHandler
    : IRequestHandler<CompleteTripCommand>
{
    private readonly IRouteUsageRepository _routeUsageRepository;
    private readonly IMobilityUnitOfWork _unitOfWork;

    public CompleteTripCommandHandler(
        IRouteUsageRepository routeUsageRepository,
        IMobilityUnitOfWork unitOfWork)
    {
        _routeUsageRepository = routeUsageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CompleteTripCommand request,
        CancellationToken cancellationToken)
    {
        var routeUsage = await _routeUsageRepository.GetByIdAsync(
            request.UsageId,
            request.UserId,
            cancellationToken);

        if (routeUsage is null)
            throw new NotFoundException("El trayecto no existe.");

        JsonDocument? gpsData = null;
        List<(double Lat, double Lng, long Ts)> samples = new();

        if (!string.IsNullOrWhiteSpace(request.GpsDataJson))
        {
            try
            {
                gpsData = JsonDocument.Parse(request.GpsDataJson);
                if (gpsData.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in gpsData.RootElement.EnumerateArray())
                    {
                        double lat = 0, lng = 0;
                        long ts = 0;
                        bool hasLat = false, hasLng = false;

                        if (item.TryGetProperty("lat", out var pLat) && pLat.TryGetDouble(out lat)) hasLat = true;
                        else if (item.TryGetProperty("latitude", out var pLat2) && pLat2.TryGetDouble(out lat)) hasLat = true;

                        if (item.TryGetProperty("lng", out var pLng) && pLng.TryGetDouble(out lng)) hasLng = true;
                        else if (item.TryGetProperty("longitude", out var pLng2) && pLng2.TryGetDouble(out lng)) hasLng = true;

                        if (item.TryGetProperty("timestamp", out var pTs) && pTs.TryGetInt64(out ts)) { }
                        else if (item.TryGetProperty("ts", out var pTs2) && pTs2.TryGetInt64(out ts)) { }

                        if (hasLat && hasLng)
                        {
                            samples.Add((lat, lng, ts));
                        }
                    }
                }
            }
            catch
            {
                // Si el JSON es inválido, gpsData se mantendrá null
            }
        }

        // 1. Cálculo y validación en Backend de la evidencia GPS
        decimal certifiedDistanceKm = 0m;
        LineString? actualRoute = null;

        if (samples.Count >= 2)
        {
            // Reconstruir LineString (X = Lng, Y = Lat para NetTopologySuite / GIS)
            var coordinates = samples.Select(s => new Coordinate(s.Lng, s.Lat)).ToArray();
            actualRoute = new LineString(coordinates) { SRID = 4326 };

            // Cálculo geodésico Haversine entre puntos sucesivos
            double totalMeters = 0;
            for (int i = 0; i < samples.Count - 1; i++)
            {
                var p1 = samples[i];
                var p2 = samples[i + 1];
                double segmentDist = CalculateHaversineMeters(p1.Lat, p1.Lng, p2.Lat, p2.Lng);

                // Filtrar ruido o saltos de teleportación (> 1000 m en intervalo corto)
                if (segmentDist > 0 && segmentDist < 1000)
                {
                    totalMeters += segmentDist;
                }
            }

            certifiedDistanceKm = Math.Round((decimal)(totalMeters / 1000.0), 2);
        }
        else if (request.ActualDistanceKm.HasValue && request.ActualDistanceKm.Value > 0)
        {
            // Si el cliente envía distancia sin telemetría, se acota
            certifiedDistanceKm = request.ActualDistanceKm.Value;
        }

        // 2. Duración
        int certifiedDurationMin = 0;
        if (request.ActualDurationMin.HasValue && request.ActualDurationMin.Value > 0)
        {
            certifiedDurationMin = request.ActualDurationMin.Value;
        }
        else if (request.EndedAt.HasValue && request.EndedAt.Value > routeUsage.StartedAt)
        {
            certifiedDurationMin = (int)Math.Max(1, Math.Round((request.EndedAt.Value - routeUsage.StartedAt).TotalMinutes));
        }

        // 3. Regla de negocio de Validación y Estado
        // Un viaje con menos de 50 metros (0.05 km) o sin muestras no certifica recorrido real
        bool hasPhysicalMovement = certifiedDistanceKm >= 0.05m;

        // Comprobación de proximidad a destino si la ruta tiene EndLocation
        bool nearDestination = false;
        var route = routeUsage.Route;
        if (route?.EndLocation != null && samples.Count > 0)
        {
            var lastSample = samples.Last();
            double distToDestM = CalculateHaversineMeters(lastSample.Lat, lastSample.Lng, route.EndLocation.Y, route.EndLocation.X);
            nearDestination = distToDestM <= 150.0; // 150 metros de tolerancia
        }
        else if (route != null && certifiedDistanceKm >= (route.DistanceKm ?? 0m) * 0.7m)
        {
            nearDestination = true;
        }

        // Cálculo de CO2 proporcional certificado
        decimal? certifiedCo2 = null;
        if (route?.Co2SavedKg != null && route.DistanceKm.HasValue && route.DistanceKm.Value > 0)
        {
            decimal ratio = Math.Min(certifiedDistanceKm / route.DistanceKm.Value, 1.0m);
            certifiedCo2 = Math.Round(route.Co2SavedKg.Value * ratio, 4);
        }
        else
        {
            certifiedCo2 = request.ActualCo2Kg;
        }

        if (hasPhysicalMovement && nearDestination)
        {
            routeUsage.Complete(
                certifiedDistanceKm,
                certifiedDurationMin,
                certifiedCo2,
                request.EndedAt ?? DateTime.UtcNow,
                actualRoute: actualRoute,
                gpsData: gpsData);
        }
        else if (hasPhysicalMovement)
        {
            // El usuario se movió pero no llegó al destino: se certifica como interrumpido
            routeUsage.Interrupt(
                certifiedDistanceKm,
                certifiedDurationMin,
                certifiedCo2,
                request.EndedAt ?? DateTime.UtcNow,
                actualRoute: actualRoute,
                gpsData: gpsData);
        }
        else
        {
            // Viaje sin movimiento físico (fraude o falso viaje)
            // Se finaliza con 0 km y 0 CO2, sin acreditar completitud de impacto
            routeUsage.Interrupt(
                0m,
                0,
                0m,
                request.EndedAt ?? DateTime.UtcNow,
                actualRoute: null,
                gpsData: gpsData);
        }

        _routeUsageRepository.Update(routeUsage);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static double CalculateHaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Radio de la Tierra en metros
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
