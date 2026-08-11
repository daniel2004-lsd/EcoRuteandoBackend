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
        public Role? PrimaryRole { get; private set; }
        public int FailedAttempts { get; private set; }
        public DateTime? LockedUntil { get; private set; }
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
            FailedAttempts = 0;
            LockedUntil = null;
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
        string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new DomainException("El nombre es obligatorio.");
            }

            FirstName = firstName.Trim();
            LastName = lastName?.Trim();
            PhoneNumber = phoneNumber?.Trim();
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
