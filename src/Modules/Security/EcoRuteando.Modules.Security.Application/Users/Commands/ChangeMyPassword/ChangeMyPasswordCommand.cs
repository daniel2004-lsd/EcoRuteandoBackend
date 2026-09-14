using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.ChangeMyPassword;

/// <summary>
/// El propio usuario cambia su contraseña confirmando la actual (CU12).
/// </summary>
public sealed record ChangeMyPasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest;
