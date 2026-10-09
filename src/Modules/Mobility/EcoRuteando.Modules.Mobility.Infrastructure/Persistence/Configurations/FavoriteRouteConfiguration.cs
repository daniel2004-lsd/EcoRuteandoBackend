using EcoRuteando.Modules.Mobility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;

public sealed class FavoriteRouteConfiguration
    : IEntityTypeConfiguration<FavoriteRoute>
{
    public void Configure(EntityTypeBuilder<FavoriteRoute> builder)
    {
        builder.ToTable("favorite_routes", "mobility");

        // PK compuesta (user_id, route_id) igual que el DDL
        builder.HasKey(fr => new { fr.UserId, fr.RouteId });

        builder.Property(fr => fr.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(fr => fr.RouteId)
            .HasColumnName("route_id")
            .IsRequired();

        builder.Property(fr => fr.Label)
            .HasColumnName("label")
            .HasMaxLength(80);

        builder.Property(fr => fr.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        // Si se borra la ruta, se borra el favorito (ON DELETE CASCADE en BD)
        builder.HasOne(fr => fr.Route)
            .WithMany()
            .HasForeignKey(fr => fr.RouteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
