using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcoRuteando.Modules.Security.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoRuteando.Modules.Security.Infrastructure.Persistence.Configurations
{
    public sealed class  RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("role_permissions", "security");
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId }); // esto es una clave primaria compuesta



            builder.Property(rp => rp.RoleId)
                .HasColumnName("role_id");

            //°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°

            builder.Property(rp=> rp.PermissionId)
                .HasColumnName("permission_id");

            builder.HasOne(rp=> rp.Role) // UN ROLE_PERMISO PERTENECE A UN ROL
                 .WithMany(r => r.RolePermissions) // UN ROL PUEDE TENER MUCHOS ROLE_PERMISOS
                 .HasForeignKey(rp => rp.RoleId); // CLAVE FORANEA HACIA LA ENTIDAD ROLE_ID

            //°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°

            builder.HasOne(rp =>rp.Permission) //
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId); // CLAVE FORANEA HACIA LA ENTIDAD PERMISSION_ID



        }
    }
}
