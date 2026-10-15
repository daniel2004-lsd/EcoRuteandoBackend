namespace EcoRuteando.Modules.Security.Application.Abstractions.Security;

/// <summary>
/// Limita las solicitudes de recuperación de contraseña por email,
/// con bloqueo escalonado (1min → 3min → 5min → 15min) cada N intentos.
/// </summary>
public interface IForgotPasswordRateLimiter
{
    /// <summary>
    /// Registra una solicitud para el email dado.
    /// Lanza TooManyRequestsException si el email está bloqueado
    /// o si esta solicitud excede el máximo permitido en la ventana.
    /// </summary>
    /// <param name="email">Email normalizado (lowercase/trim).</param>
    void RegisterAttempt(string email);

    /// <summary>Segundos restantes de bloqueo para el email (0 si no está bloqueado).</summary>
    int GetRetryAfterSeconds(string email);
}
