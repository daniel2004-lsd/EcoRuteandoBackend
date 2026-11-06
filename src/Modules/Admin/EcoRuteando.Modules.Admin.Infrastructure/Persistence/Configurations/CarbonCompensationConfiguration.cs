using EcoRuteando.Modules.Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Configurations;

public sealed class CarbonCompensationConfiguration : IEntityTypeConfiguration<CarbonCompensation>
{
    public void Configure(EntityTypeBuilder<CarbonCompensation> builder)
    {
        builder.ToTable("carbon_compensations", "mobility");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.CompanyId).HasColumnName("company_id");
        builder.Property(c => c.UserId).HasColumnName("user_id");

        builder.Property(c => c.Co2CompensatedKg)
            .HasColumnName("co2_compensated_kg")
            .HasPrecision(12, 4)
            .IsRequired();

        builder.Property(c => c.TreesEquivalent)
            .HasColumnName("trees_equivalent")
            .IsRequired();

        builder.Property(c => c.PartnerProgram)
            .HasColumnName("partner_program")
            .HasMaxLength(150)
            .HasDefaultValue("Reforestación Cuenca Río Las Ceibas (CAM Huila)")
            .IsRequired();

        builder.Property(c => c.CertificateCode)
            .HasColumnName("certificate_code")
            .HasMaxLength(50)
            .IsRequired();
        builder.HasIndex(c => c.CertificateCode).IsUnique();

        builder.Property(c => c.CompensationStatus)
            .HasColumnName("compensation_status")
            .HasMaxLength(30)
            .HasDefaultValue("certified")
            .IsRequired();

        builder.Property(c => c.Notes)
            .HasColumnName("notes");

        builder.Property(c => c.CertifiedAt)
            .HasColumnName("certified_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();
    }
}
