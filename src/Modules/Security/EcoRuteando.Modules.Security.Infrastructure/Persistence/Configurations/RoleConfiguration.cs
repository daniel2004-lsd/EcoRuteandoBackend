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
    public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles", "security"); 
            builder.HasKey(r => r.Id); // define cual es la llave primaria de la tabla
            builder.Property(r => r.Id) // trata de decir que va a configurar la propiedad Id de la entidad Role
                .HasColumnName("role_id"); // la propiedad id se gurda en la columna role_id de la tabla roles
            builder.Property(r => r.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(50);


            builder.Property(r => r.Description)
                .HasColumnName("description")
                .HasMaxLength(255);
            builder.HasIndex(r => r.Name) // define un indice en la columna name de la tabla roles
                .IsUnique(); // define un indice unico en la columna name de la tabla roles
        }
            
    }

}
