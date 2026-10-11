using EcoRuteando.Modules.Mobility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;

public sealed class ReportVoteConfiguration : IEntityTypeConfiguration<ReportVote>
{
    public void Configure(EntityTypeBuilder<ReportVote> builder)
    {
        builder.ToTable("report_votes", "community");

        builder.HasKey(v => new { v.UserId, v.ReportId });

        builder.Property(v => v.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(v => v.ReportId)
            .HasColumnName("report_id")
            .IsRequired();

        builder.Property(v => v.Confirms)
            .HasColumnName("confirms")
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");
    }
}
