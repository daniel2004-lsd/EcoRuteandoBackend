using EcoRuteando.Modules.Admin.Domain.Entities;

namespace EcoRuteando.Modules.Admin.Application.Support.Dtos;

public record SupportMessageDto(
    Guid Id,
    Guid TicketId,
    Guid? SenderId,
    bool IsAdmin,
    string Message,
    string? AttachmentUrl,
    DateTime CreatedAt)
{
    public static SupportMessageDto FromEntity(SupportMessage entity) =>
        new(
            entity.Id,
            entity.TicketId,
            entity.SenderId,
            entity.IsAdmin,
            entity.Message,
            entity.AttachmentUrl,
            entity.CreatedAt);
}

public record SupportTicketDto(
    Guid Id,
    Guid? UserId,
    Guid? AdminId,
    string Subject,
    string Status,
    short Priority,
    DateTime CreatedAt,
    DateTime? ClosedAt,
    IReadOnlyList<SupportMessageDto> Messages,
    string? UserEmail = null,
    string? UserName = null)
{
    public static SupportTicketDto FromEntity(SupportTicket entity, string? userEmail = null, string? userName = null) =>
        new(
            entity.Id,
            entity.UserId,
            entity.AdminId,
            entity.Subject,
            entity.Status.ToString().ToLowerInvariant(),
            entity.Priority,
            entity.CreatedAt,
            entity.ClosedAt,
            entity.Messages.Select(SupportMessageDto.FromEntity).ToList(),
            userEmail,
            userName);
}
