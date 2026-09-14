using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.UpdateMyProfile;

/// <summary>
/// El propio usuario actualiza sus datos personales confirmando
/// su identidad con la contraseña actual (CU12/RF5).
/// </summary>
public sealed record UpdateMyProfileCommand(
    Guid UserId,
    string FirstName,
    string? LastName,
    string Email,
    string? PhoneNumber,
    string CurrentPassword
) : IRequest<UpdateMyProfileResponse>;
