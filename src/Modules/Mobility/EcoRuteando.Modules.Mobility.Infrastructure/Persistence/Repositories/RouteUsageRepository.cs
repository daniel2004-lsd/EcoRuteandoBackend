using EcoRuteando.Modules.Mobility.Domain.Entities;
using EcoRuteando.Modules.Mobility.Domain.Enums;
using EcoRuteando.Modules.Mobility.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Repositories;

public sealed class RouteUsageRepository : IRouteUsageRepository
{
    private readonly MobilityDbContext _dbContext;

    public RouteUsageRepository(MobilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RouteUsage?> GetByIdAsync(
        Guid usageId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RouteUsages
            .AsNoTracking()
            .Include(ru => ru.Route)
            .FirstOrDefaultAsync(
                ru => ru.Id == usageId && ru.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<RouteUsage>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RouteUsages
            .AsNoTracking()
            .Include(ru => ru.Route)
            .Where(ru => ru.UserId == userId)
            .OrderByDescending(ru => ru.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        RouteUsage routeUsage,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RouteUsages.AddAsync(routeUsage, cancellationToken);
    }

    public async Task<bool> HasCompletedUsageAsync(
        Guid userId,
        Guid routeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RouteUsages
            .AsNoTracking()
            .AnyAsync(
                ru => ru.UserId == userId
                    && ru.RouteId == routeId
                    && ru.Completed,
                cancellationToken);
    }

    public async Task<RouteUsageAnalytics> GetAnalyticsAsync(
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.RouteUsages
            .AsNoTracking()
            .Where(ru => ru.Completed);

        if (from.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
            query = query.Where(ru => ru.StartedAt >= fromUtc);
        }

        if (to.HasValue)
        {
            // Incluir todo el día "to": hasta final del día UTC.
            var toEndOfDayUtc = DateTime.SpecifyKind(
                to.Value.Date.AddDays(1).AddTicks(-1),
                DateTimeKind.Utc);
            query = query.Where(ru => ru.StartedAt <= toEndOfDayUtc);
        }

        // Sólo se traen las columnas necesarias para las agregaciones (evita
        // cargar polys/gps_data). Las distinciones y agrupaciones se hacen en
        // memoria para garantizar traducción estable de EF Core a PostgreSQL.
        var rows = await query
            .Select(ru => new
            {
                ru.UserId,
                ru.RouteId,
                ru.TransportMode,
                ru.StartedAt,
                ru.ActualCo2Kg,
                ru.ActualDistanceKm
            })
            .ToListAsync(cancellationToken);

        var completedTrips = rows.Count;
        var activeUsers = rows.Select(r => r.UserId).Distinct().Count();
        var routesConsulted = rows.Select(r => r.RouteId).Distinct().Count();
        var totalCo2Kg = rows.Sum(r => r.ActualCo2Kg ?? 0m);
        var averageDistanceKm = completedTrips > 0
            ? rows.Average(r => (double)(r.ActualDistanceKm ?? 0m))
            : 0d;

        var byTransportMode = rows
            .GroupBy(r => r.TransportMode?.ToPgName())
            .Select(g => new TransportModeStat(
                g.Key,
                g.Count(),
                g.Sum(r => r.ActualCo2Kg ?? 0m),
                g.Select(r => r.UserId).Distinct().Count()))
            .OrderByDescending(s => s.Trips)
            .ToList();

        var monthly = rows
            .GroupBy(r => r.StartedAt.ToString("yyyy-MM"))
            .Select(g => new MonthStat(
                g.Key,
                g.Count(),
                g.Sum(r => r.ActualCo2Kg ?? 0m)))
            .OrderBy(m => m.Period)
            .ToList();

        return new RouteUsageAnalytics(
            completedTrips,
            activeUsers,
            routesConsulted,
            totalCo2Kg,
            averageDistanceKm,
            byTransportMode,
            monthly);
    }

    public void Update(RouteUsage routeUsage)
    {
        _dbContext.RouteUsages.Update(routeUsage);
    }
}
