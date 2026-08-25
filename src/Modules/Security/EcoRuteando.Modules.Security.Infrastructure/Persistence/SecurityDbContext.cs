
using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Infrastructure.Persistence.Configurations;
using EcoRuteando.Shared.Abstractions;
using EcoRuteando.Shared.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence
{
    public class SecurityDbContext : DbContext, IUnitOfWork
    {
        public SecurityDbContext(DbContextOptions<SecurityDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<PasswordRecovery> PasswordRecoveries => Set<PasswordRecovery>();

        public DbSet<Permission> Permissions => Set<Permission>();

        public DbSet<UserRole> UserRoles => Set<UserRole>();

        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<SecurityPolicy> SecurityPolicies => Set<SecurityPolicy>();

        public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();

        public DbSet<OAuthAccount> OAuthAccounts => Set<OAuthAccount>();

        public DbSet<Session> Sessions => Set<Session>();

        public DbSet<TwoFactorAuth> TwoFactorAuths => Set<TwoFactorAuth>();

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresEnum(
                "security",
                "two_factor_method",
                new[] { "totp", "sms", "email" });

            modelBuilder.HasPostgresEnum(
                "security",
                "error_level",
                new[] { "info", "warning", "error", "critical" });

            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new PasswordRecoveryConfiguration());
            modelBuilder.ApplyConfiguration(new SecurityPolicyConfiguration());
            modelBuilder.ApplyConfiguration(new EmailVerificationConfiguration());
            modelBuilder.ApplyConfiguration(new OAuthAccountConfiguration());
            modelBuilder.ApplyConfiguration(new SessionConfiguration());
            modelBuilder.ApplyConfiguration(new TwoFactorAuthConfiguration());
            modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
            modelBuilder.ApplyConfiguration(new ErrorLogConfiguration());
        }
    }
}
