using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Shared.Exceptions
{
    public sealed class UnauthorizedException : DomainException
    {
        /// <summary>Intentos restantes antes del próximo bloqueo (null si no aplica).</summary>
        public int? AttemptsRemaining { get; }

        /// <summary>Segundos restantes del bloqueo actual (null si no está bloqueado).</summary>
        public int? RetryAfterSeconds { get; }

        public UnauthorizedException(string message)
            : base(message)
        {
        }

        public UnauthorizedException(
            string message,
            int? attemptsRemaining = null,
            int? retryAfterSeconds = null)
            : base(message)
        {
            AttemptsRemaining = attemptsRemaining;
            RetryAfterSeconds = retryAfterSeconds;
        }
    }
}
