using EcoRuteando.Modules.Admin.Domain.Entities;
using EcoRuteando.Modules.Admin.Domain.Enums;
using EcoRuteando.Modules.Admin.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Repositories;

public class SupportTicketRepository : ISupportTicketRepository
{
    private readonly AdminDbContext _context;

    public SupportTicketRepository(AdminDbContext context)
    {
        _context = context;
    }

    public async Task<SupportTicket?> GetByIdAsync(Guid id, bool includeMessages = true, CancellationToken cancellationToken = default)
    {
        var query = _context.SupportTickets.AsQueryable();
        if (includeMessages)
        {
            query = query.Include(t => t.Messages);
        }

        return await query.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.SupportTickets
            .Include(t => t.Messages)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetAllAsync(SupportTicketStatus? status = null, short? priority = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SupportTickets
            .Include(t => t.Messages)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SupportTicket ticket, CancellationToken cancellationToken = default)
    {
        await _context.SupportTickets.AddAsync(ticket, cancellationToken);
    }

    public async Task AddMessageAsync(SupportMessage message, CancellationToken cancellationToken = default)
    {
        await _context.SupportMessages.AddAsync(message, cancellationToken);
    }

    public async Task<Dictionary<Guid, (string Email, string Name)>> GetUserDetailsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<Guid, (string Email, string Name)>();
        var distinctIds = userIds.Distinct().ToArray();
        if (distinctIds.Length == 0) return dict;

        var conn = _context.Database.GetDbConnection();
        var shouldClose = false;
        if (conn.State != System.Data.ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken);
            shouldClose = true;
        }

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, email, first_name, last_name FROM security.users WHERE id = ANY(@ids)";
            var param = cmd.CreateParameter();
            param.ParameterName = "ids";
            param.Value = distinctIds;
            cmd.Parameters.Add(param);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var id = reader.GetGuid(0);
                var email = reader.IsDBNull(1) ? "" : reader.GetString(1);
                var first = reader.IsDBNull(2) ? "" : reader.GetString(2);
                var last = reader.IsDBNull(3) ? "" : reader.GetString(3);
                var fullName = $"{first} {last}".Trim();
                dict[id] = (email, string.IsNullOrWhiteSpace(fullName) ? email : fullName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo detalles de usuario para soporte: {ex.Message}");
        }
        finally
        {
            if (shouldClose)
            {
                await conn.CloseAsync();
            }
        }

        return dict;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
