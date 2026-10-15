namespace EcoRuteando.Modules.Security.Application.Users.Commands.UpdateMyPhoto;

public sealed record UpdateMyPhotoResponse(
    Guid UserId,
    string? ProfilePhotoUrl
);
