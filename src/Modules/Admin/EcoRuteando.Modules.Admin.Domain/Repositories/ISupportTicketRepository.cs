using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Enums;

namespace EcoRuteando.Modules.Admin.Domain.Repositories;

public interface ISupportTicketRepository
{
    Task<SupportTicket?> GetByIdAsync(Guid id, bool includeMessages = true, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupportTicket>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SupportTicket>> GetAllAsync(SupportTicketStatus? status = null, short? priority = null, CancellationToken cancellationToken = default);
    Task AddAsync(SupportTicket ticket, CancellationToken cancellationToken = default);
    Task AddMessageAsync(SupportMessage message, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, (string Email, string Name)>> GetUserDetailsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
