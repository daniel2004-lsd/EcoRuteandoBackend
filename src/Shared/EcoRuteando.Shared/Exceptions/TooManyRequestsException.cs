using System;

namespace EcoRuteando.Shared.Exceptions
{
    /// <summary>
    /// Excepción 429: demasiadas solicitudes. Incluye segundos de espera
    /// para que el cliente muestre el countdown (retryAfterSeconds).
    /// </summary>
    public sealed class TooManyRequestsException : DomainException
    {
        /// <summary>Segundos que el cliente debe esperar antes de reintentar.</summary>
        public int RetryAfterSeconds { get; }

        public TooManyRequestsException(string message, int retryAfterSeconds)
            : base(message)
        {
            RetryAfterSeconds = retryAfterSeconds;
        }
    }
}
