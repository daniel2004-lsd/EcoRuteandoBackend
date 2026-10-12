namespace EcoRuteando.Modules.Admin.Domain.Entities;

public class SupportMessage
{
    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public Guid? SenderId { get; private set; }
    public bool IsAdmin { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string? AttachmentUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public SupportTicket? Ticket { get; private set; }

    private SupportMessage() { }

    public static SupportMessage Create(
        Guid ticketId,
        Guid? senderId,
        bool isAdmin,
        string message,
        string? attachmentUrl = null)
    {
        return new SupportMessage
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            SenderId = senderId,
            IsAdmin = isAdmin,
            Message = message,
            AttachmentUrl = attachmentUrl,
            CreatedAt = DateTime.UtcNow
        };
    }
}
