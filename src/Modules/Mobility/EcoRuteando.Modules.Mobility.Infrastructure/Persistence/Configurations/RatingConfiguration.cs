using EcoRuteando.Modules.Mobility.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Mobility.Infrastructure.Persistence.Configurations;

public sealed class RatingConfiguration
    : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.ToTable("ratings", "mobility");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.RouteId)
            .HasColumnName("route_id")
            .IsRequired();

        builder.Property(r => r.UsageId)
            .HasColumnName("usage_id");

        builder.Property(r => r.RatingValue)
            .HasColumnName("rating")
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasColumnName("comment");

        builder.Property(r => r.HelpfulCount)
            .HasColumnName("helpful_count")
            .HasDefaultValue(0);

        builder.Property(r => r.IsPublished)
            .HasColumnName("is_published")
            .HasDefaultValue(true);

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()");

        // Un usuario solo puede calificar una vez cada ruta (UNIQUE(user_id, route_id) en BD)
        builder.HasIndex(r => new { r.UserId, r.RouteId })
            .IsUnique();

        // Si se borra la ruta, se borran sus calificaciones (ON DELETE CASCADE en BD)
        builder.HasOne(r => r.Route)
            .WithMany()
            .HasForeignKey(r => r.RouteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
