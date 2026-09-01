using EcoRuteando.Modules.Mobility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;

public sealed class RoutePoiConfiguration : IEntityTypeConfiguration<RoutePoi>
{
    public void Configure(EntityTypeBuilder<RoutePoi> builder)
    {
        builder.ToTable("route_poi", "mobility");

        // PK compuesta (route_id, poi_id) igual que el DDL
        builder.HasKey(rp => new { rp.RouteId, rp.PoiId });

        builder.Property(rp => rp.RouteId)
            .HasColumnName("route_id");

        builder.Property(rp => rp.PoiId)
            .HasColumnName("poi_id");

        builder.Property(rp => rp.SortOrder)
            .HasColumnName("sort_order");
    }
}
