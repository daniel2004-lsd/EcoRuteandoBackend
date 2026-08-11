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
    public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("user_roles", "security");
            builder.HasKey(ur => new { ur.UserId, ur.RoleId });
            //°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°°

            builder.Property(ur=> ur.UserId)
                .HasColumnName("user_id");

            builder.Property(ur=> ur.RoleId)
                .HasColumnName("role_id");


            builder.HasOne(ur=> ur.Role)
                .WithMany(r=> r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            builder.HasOne(ur=> ur.User)
                .WithMany(u=> u.UserRoles)
                .HasForeignKey(ur => ur.UserId);



        }   
    }
}
