using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.UpdateMyPhoto;

/// <summary>
/// El usuario autenticado actualiza su foto de perfil.
/// Se envía como data URI base64 (ej. "data:image/jpeg;base64,...").
/// Enviar null o vacío para eliminar la foto.
/// </summary>
public sealed record UpdateMyPhotoCommand(
    Guid UserId,
    string? PhotoBase64
) : IRequest<UpdateMyPhotoResponse>;
