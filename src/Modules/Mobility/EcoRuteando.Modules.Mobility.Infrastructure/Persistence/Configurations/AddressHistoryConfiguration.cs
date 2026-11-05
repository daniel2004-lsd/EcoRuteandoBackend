using EcoRuteando.Modules.Mobility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;

public sealed class AddressHistoryConfiguration : IEntityTypeConfiguration<AddressHistory>
{
    public void Configure(EntityTypeBuilder<AddressHistory> builder)
    {
        builder.ToTable("address_history", "mobility");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.SearchText)
            .HasColumnName("search_text")
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(a => a.PlaceId)
            .HasColumnName("place_id")
            .HasMaxLength(255);

        builder.Property(a => a.Location)
            .HasColumnName("location")
            .HasColumnType("geography(Point, 4326)")
            .IsRequired();

        builder.Property(a => a.SearchType)
            .HasColumnName("search_type")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");
    }
}
