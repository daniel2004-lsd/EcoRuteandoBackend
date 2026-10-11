using EcoRuteando.Modules.Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Admin.Infrastructure.Persistence.Configurations;

public class DailyStatisticConfiguration : IEntityTypeConfiguration<DailyStatistic>
{
    public void Configure(EntityTypeBuilder<DailyStatistic> builder)
    {
        builder.ToTable("daily_statistics", "admin");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.StatisticDate)
            .HasColumnName("statistic_date")
            .HasColumnType("date")
            .IsRequired();

        builder.HasIndex(s => s.StatisticDate)
            .IsUnique();

        builder.Property(s => s.TotalUsers)
            .HasColumnName("total_users")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(s => s.NewUsers)
            .HasColumnName("new_users")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(s => s.ActiveUsers)
            .HasColumnName("active_users")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(s => s.TotalRoutes)
            .HasColumnName("total_routes")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(s => s.CompletedRoutes)
            .HasColumnName("completed_routes")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(s => s.TotalCommunities)
            .HasColumnName("total_communities")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(s => s.TotalReports)
            .HasColumnName("total_reports")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(s => s.TotalCo2Saved)
            .HasColumnName("total_co2_saved")
            .HasPrecision(12, 2)
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(s => s.TotalDistanceKm)
            .HasColumnName("total_distance_km")
            .HasPrecision(12, 2)
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
