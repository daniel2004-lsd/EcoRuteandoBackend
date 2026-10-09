using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Mobility.Application.AddressHistory;

public sealed class GetUserAddressHistoryHandler
    : IRequestHandler<GetUserAddressHistoryQuery, IReadOnlyList<GetUserAddressHistoryResponse>>
{
    private readonly IMobilityDbContext _dbContext;

    public GetUserAddressHistoryHandler(IMobilityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<GetUserAddressHistoryResponse>> Handle(
        GetUserAddressHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var addresses = await _dbContext.AddressHistory
            .Where(a => a.UserId == request.UserId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new GetUserAddressHistoryResponse
            {
                Id = a.Id,
                SearchText = a.SearchText,
                PlaceId = a.PlaceId,
                Latitude = a.Location.Value.Y,
                Longitude = a.Location.Value.X,
                SearchType = a.SearchType,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return addresses;
    }
}