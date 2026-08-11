using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EcoRuteando.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;




namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Configurations
{
    public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("permissions","security");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                    .HasColumnName("permission_id");
           //----------------------------------------------
            builder.Property(p => p.Name)
                    .HasColumnName("name")
                    .IsRequired()
                    .HasMaxLength(80);
            builder.HasIndex(p => p.Name)
                    .IsUnique();
                    
           //----------------------------------------------
            builder.Property(p => p.Description)
                    .HasColumnName("description")
                    .HasMaxLength(255);
            //----------------------------------------------
            builder.Property(p => p.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired()
                    .HasDefaultValueSql("now()");


            builder.Property(p => p.UpdatedAt)
                    .HasColumnName("updated_at")
                    .IsRequired()
                    .HasDefaultValueSql("now()");






        }
    }
}
