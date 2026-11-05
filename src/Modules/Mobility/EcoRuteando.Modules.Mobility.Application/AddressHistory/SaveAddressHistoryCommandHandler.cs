using EcoRuteando.Modules.Mobility.Application.Abstractions;
using MediatR;
using NetTopologySuite.Geometries;
using AddressHistoryEntity = EcoRuteando.Modules.Mobility.Domain.Entities.AddressHistory;

namespace EcoRuteando.Modules.Mobility.Application.AddressHistory;

public sealed class SaveAddressHistoryCommandHandler
    : IRequestHandler<SaveAddressHistoryCommand, Guid>
{
    private readonly IMobilityDbContext _dbContext;

    public SaveAddressHistoryCommandHandler(IMobilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(
        SaveAddressHistoryCommand request,
        CancellationToken cancellationToken)
    {
        var point = new Point((double)request.Longitude, (double)request.Latitude) { SRID = 4326 };

        var normalizedSearchType = request.SearchType?.Trim().ToLowerInvariant() switch
        {
            "route_origin" or "origin" => "origin",
            "route_destination" or "destination" => "destination",
            _ => "search"
        };

        var entry = new AddressHistoryEntity(
            request.UserId,
            request.SearchText,
            request.PlaceId,
            point,
            normalizedSearchType);

        _dbContext.AddressHistory.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entry.Id;
    }
}
