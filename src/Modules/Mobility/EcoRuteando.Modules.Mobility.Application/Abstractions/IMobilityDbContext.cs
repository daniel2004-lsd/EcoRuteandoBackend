using EcoRuteando.Shared.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using AddressHistoryEntity = EcoRuteando.Modules.Mobility.Domain.Entities.AddressHistory;

namespace EcoRuteando.Modules.Mobility.Application.Abstractions;

/// <summary>
/// Contrato del DbContext de Mobility que expone el DbSet de AddressHistory (HU-23).
/// Implementado por MobilityDbContext en Infrastructure.
/// </summary>
public interface IMobilityDbContext : IMobilityUnitOfWork
{
    DbSet<AddressHistoryEntity> AddressHistory { get; }
}
