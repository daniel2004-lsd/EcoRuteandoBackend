using EcoRuteando.Modules.Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies", "admin");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Nit)
            .HasColumnName("nit")
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(c => c.Nit).IsUnique();

        builder.Property(c => c.BusinessName)
            .HasColumnName("business_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Sector)
            .HasColumnName("sector")
            .HasMaxLength(50)
            .HasDefaultValue("passenger_transport")
            .IsRequired();

        builder.Property(c => c.MonthlyCo2BudgetKg)
            .HasColumnName("monthly_co2_budget_kg")
            .HasPrecision(12, 2)
            .HasDefaultValue(50000m)
            .IsRequired();

        builder.Property(c => c.ContactEmail)
            .HasColumnName("contact_email")
            .HasMaxLength(120);

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasMany(c => c.FleetTypes)
            .WithOne()
            .HasForeignKey(f => f.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
