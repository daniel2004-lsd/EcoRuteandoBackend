using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcoRuteando.Shared.BaseClasses;
using EcoRuteando.Shared.Exceptions;

namespace EcoRuteando.Modules.Security.Domain.Entities
{
    public sealed class User : Entity<Guid>
    {
        public string FirstName {get; private set;} = string.Empty;
        public string? LastName {get; private set;}
        public string Email {get; private set;} = string.Empty;
        public string? PasswordHash {get; private set;}
        public string Status { get; private set; } = "active";
        public string? PhoneNumber {get; private set; }
        public bool AcceptedTerms { get; private set; }
        public DateTime? TermsAcceptedAt { get; private set; }
        public string? ProfilePhotoUrl { get; private set; }
        public bool EmailVerified { get; private set; }
        public bool IsGuest { get; private set; }
        public string PreferredLanguage { get; private set; } = "es";
        public string UiTheme { get; private set; } = "light";
        public string? PrimaryColor { get; private set; }
        public Role? PrimaryRole { get; private set; }
        public int FailedAttempts { get; private set; }
        public DateTime? LockedUntil { get; private set; }
        public DateTime? LastLogin { get; private set; }
        public DateTime? DeletionRequestedAt { get; private set; }
        public Guid? PrimaryRoleId { get; private set; }

        public ICollection<UserRole> UserRoles { get; private set; }
            = new List<UserRole>();

        public ICollection<RefreshToken> RefreshTokens { get; private set; }
            = new List<RefreshToken>();


        private User()
        {
        }
        
        public User(string firstName, string? lastName, string email, string? passwordHash ,string? phoneNumber)
        {
            if(string.IsNullOrWhiteSpace(firstName))
            {
                throw new DomainException("El nombre es obligatorio");
            }
            if(string.IsNullOrWhiteSpace(email))
            {
                throw new DomainException("El correo electrónico es obligatorio.");
            }


            FirstName = firstName.Trim();
            LastName = lastName?.Trim();
            Email = email.Trim().ToLowerInvariant();
            PasswordHash = passwordHash;
            PhoneNumber = phoneNumber?.Trim();
            Status = "active";
            AcceptedTerms = false;
            TermsAcceptedAt = null;
            ProfilePhotoUrl = null;
            EmailVerified = false;
            IsGuest = false;
            PreferredLanguage = "es";
            UiTheme = "light";
            PrimaryColor = null;
            FailedAttempts = 0;
            LockedUntil = null;
            LastLogin = null;
            DeletionRequestedAt = null;
            PrimaryRoleId = null;


        }
        public void AcceptTerms()
        {
            AcceptedTerms = true;
            TermsAcceptedAt = DateTime.UtcNow; // Set the acceptance date to the current UTC time
        }

        public void VerifyEmail()
        {
            EmailVerified = true;
        }
        public void AssignPrimaryRole(Role role)
        {
            if (role is null)
            {
                throw new DomainException("El rol es obligatorio.");
            }

            PrimaryRole = role;
            PrimaryRoleId = role.Id;
        }
        
        public void Update(
         string firstName,
        string? lastName,
        string? phoneNumber,
        string? primaryColor = null)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new DomainException("El nombre es obligatorio.");
            }

            if (primaryColor is not null
                && !System.Text.RegularExpressions.Regex.IsMatch(
                    primaryColor, "^#[0-9A-Fa-f]{6}$"))
            {
                throw new DomainException(
                    "El color primario debe ser un hexadecimal válido (ej. #1ABC9C).");
            }

            FirstName = firstName.Trim();
            LastName = lastName?.Trim();
            PhoneNumber = phoneNumber?.Trim();
            PrimaryColor = primaryColor;
        }

        public void RecordLogin()
        {
            LastLogin = DateTime.UtcNow;
        }

        public void RequestDeletion()
        {
            DeletionRequestedAt = DateTime.UtcNow;
            Status = "deleted";
        }


        public bool IsLocked =>
            LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

        /// <summary>
        /// Duraciones de bloqueo escalonado (en orden ascendente de severidad).
        /// Nivel 0 = 30s, 1 = 1 min, 2 = 5 min, 3+ = 15 min.
        /// </summary>
        private static readonly int[] LockoutEscalationSeconds =
            { 30, 60, 300, 900 };

        /// <summary>
        /// Devuelve el tiempo restante de bloqueo en segundos (0 si no está bloqueado).
        /// </summary>
        public int GetLockedSecondsRemaining()
        {
            if (!IsLocked || !LockedUntil.HasValue)
            {
                return 0;
            }

            var remaining = (int)(LockedUntil.Value - DateTime.UtcNow).TotalSeconds;

            return Math.Max(0, remaining);
        }

        /// <summary>
        /// Nivel de escalada del bloqueo, derivado del número acumulado de intentos
        /// fallidos. No requiere columna extra: cada ciclo completo de
        /// MaxAttempts intentos sube un nivel.
        /// </summary>
        public int GetLockoutLevel(int maxAttempts)
        {
            var safeMax = Math.Max(1, maxAttempts);

            return Math.Max(0, (FailedAttempts - 1) / safeMax);
        }

        /// <summary>
        /// Cuántos intentos quedan antes del siguiente bloqueo.
        /// </summary>
        public int GetAttemptsRemaining(int maxAttempts)
        {
            var safeMax = Math.Max(1, maxAttempts);
            var attemptsInCycle = FailedAttempts % safeMax;

            return safeMax - attemptsInCycle;
        }

        public void IncrementFailedAttempts(int maxAttempts, int ignoredLockoutMinutes)
        {
            FailedAttempts++;
            UpdatedAt = DateTime.UtcNow;

            if (FailedAttempts >= maxAttempts)
            {
                LockWithEscalation(maxAttempts);
            }
        }

        private void LockWithEscalation(int maxAttempts)
        {
            var level = GetLockoutLevel(maxAttempts);
            var seconds = LockoutEscalationSeconds[
                Math.Min(level, LockoutEscalationSeconds.Length - 1)];

            LockedUntil = DateTime.UtcNow.AddSeconds(seconds);
            UpdatedAt = DateTime.UtcNow;
        }

        public void ResetFailedAttempts()
        {
            FailedAttempts = 0;
            LockedUntil = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePassword(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new DomainException("La contraseña es obligatoria.");
            }

            PasswordHash = passwordHash;
            FailedAttempts = 0;
            LockedUntil = null;
            UpdatedAt = DateTime.UtcNow;
        }

    }


}
