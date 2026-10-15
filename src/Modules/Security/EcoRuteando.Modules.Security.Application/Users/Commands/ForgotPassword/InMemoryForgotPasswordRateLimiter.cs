using System.Collections.Concurrent;
using EcoRuteando.Modules.Security.Application.Abstractions.Security;
using EcoRuteando.Shared.Exceptions;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.ForgotPassword;

/// <summary>
/// Limitador en memoria por email para forgot-password.
/// - Máximo 4 solicitudes cada 10 minutos por email.
/// - Al superarlo: bloqueo escalonado 60s → 180s → 300s → 900s.
/// - Singleton: el estado sobrevive entre requests.
/// </summary>
public sealed class InMemoryForgotPasswordRateLimiter : IForgotPasswordRateLimiter
{
    private const int MaxAttemptsPerWindow = 4;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

    // Escalación: nivel 0 = 1min, nivel 1 = 3min, nivel 2 = 5min, nivel 3+ = 15min
    private static readonly int[] EscalationSeconds = { 60, 180, 300, 900 };

    private sealed class Entry
    {
        public List<DateTime> RequestTimes { get; } = new();
        public int LockLevel { get; set; }
        public DateTime? LockedUntil { get; set; }
    }

    private readonly ConcurrentDictionary<string, Entry> _entries = new();
    private readonly object _lock = new();

    public void RegisterAttempt(string email)
    {
        var key = Normalize(email);
        var now = DateTime.UtcNow;

        lock (_lock)
        {
            var entry = _entries.GetOrAdd(key, _ => new Entry());

            // 1) Si está bloqueado ahora → lanzar con tiempo restante
            if (entry.LockedUntil.HasValue && entry.LockedUntil.Value > now)
            {
                var remaining = (int)Math.Ceiling((entry.LockedUntil.Value - now).TotalSeconds);
                throw new TooManyRequestsException(
                    "Demasiadas solicitudes de recuperación. Espera antes de reintentar.",
                    remaining);
            }

            // 2) Si el bloqueo ya expiró, limpiar estado
            if (entry.LockedUntil.HasValue && entry.LockedUntil.Value <= now)
            {
                entry.LockedUntil = null;
                entry.RequestTimes.Clear();
            }

            // 3) Registrar este intento y limpiar viejos (ventana deslizante)
            entry.RequestTimes.Add(now);
            entry.RequestTimes.RemoveAll(t => now - t > Window);

            // 4) ¿Excede el máximo en la ventana?
            if (entry.RequestTimes.Count > MaxAttemptsPerWindow)
            {
                // Subir de nivel y bloquear
                entry.LockLevel++;
                var level = Math.Min(entry.LockLevel - 1, EscalationSeconds.Length - 1);
                var lockSeconds = EscalationSeconds[level];

                entry.LockedUntil = now.AddSeconds(lockSeconds);
                entry.RequestTimes.Clear();

                var msg = entry.LockLevel switch
                {
                    1 => "Demasiadas solicitudes. Intenta de nuevo en 1 minuto.",
                    2 => "Sigue excediendo el límite. Espera 3 minutos.",
                    3 => "Sigue excediendo el límite. Espera 5 minutos.",
                    _ => "Sigue excediendo el límite. Espera 15 minutos."
                };

                throw new TooManyRequestsException(msg, lockSeconds);
            }
        }
    }

    public int GetRetryAfterSeconds(string email)
    {
        var key = Normalize(email);
        if (!_entries.TryGetValue(key, out var entry))
            return 0;

        lock (_lock)
        {
            if (entry.LockedUntil.HasValue && entry.LockedUntil.Value > DateTime.UtcNow)
            {
                return (int)Math.Ceiling((entry.LockedUntil.Value - DateTime.UtcNow).TotalSeconds);
            }
            return 0;
        }
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
