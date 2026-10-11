using EcoRuteando.Modules.Admin.Domain.Enums;

namespace EcoRuteando.Modules.Admin.Domain.Entities;

public class SupportTicket
{
    private readonly List<SupportMessage> _messages = new();

    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? AdminId { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public SupportTicketStatus Status { get; private set; }
    public short Priority { get; private set; } // 1 to 4, default 2
    public DateTime CreatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    public IReadOnlyCollection<SupportMessage> Messages => _messages.AsReadOnly();

    private SupportTicket() { }

    public static SupportTicket Create(
        Guid? userId,
        string subject,
        short priority = 2)
    {
        return new SupportTicket
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Subject = subject,
            Status = SupportTicketStatus.Pending,
            Priority = (short)Math.Clamp((int)priority, 1, 4),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddMessage(Guid? senderId, bool isAdmin, string message, string? attachmentUrl = null)
    {
        var msg = SupportMessage.Create(Id, senderId, isAdmin, message, attachmentUrl);
        _messages.Add(msg);
    }

    public void AssignAdmin(Guid adminId)
    {
        if (adminId == UserId)
        {
            throw new InvalidOperationException("Un administrador no puede auto-atender su propio ticket.");
        }
        AdminId = adminId;
        if (Status == SupportTicketStatus.Pending)
        {
            Status = SupportTicketStatus.InProgress;
        }
    }

    public void UpdateStatus(SupportTicketStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == SupportTicketStatus.Closed || newStatus == SupportTicketStatus.Resolved)
        {
            ClosedAt = DateTime.UtcNow;
        }
        else
        {
            ClosedAt = null;
        }
    }
}
