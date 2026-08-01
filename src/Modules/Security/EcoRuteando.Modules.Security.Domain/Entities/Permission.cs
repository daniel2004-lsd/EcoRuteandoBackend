using EcoRuteando.Shared.BaseClasses;
using EcoRuteando.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Domain.Entities
{
    public sealed class Permission : Entity<int>
    {
        public string Name { get; private set; } = string.Empty; // string.Empty me permite inicializar con una cadena vacia 
        public string? Description {get; private set;}
        public ICollection<RolePermission> RolePermissions { get; private set; } = [];

        private Permission() {
        }
        public Permission(string name, string? description)
        {

            if(string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("El nombre del permiso el obligatorio "); // ArgumentException me permite lanzar una excepcion cuando el argumento no es valido
            }
            Name = name.Trim(); // Trim me permite eliminar los espacios en blanco al inicio y al final de la cadena
            Description = description;
            CreatedAt = DateTime.UtcNow;
        }
        public void Update(
            string name,
            string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("El nombre del permiso es obligatorio.");
            }

            Name = name.Trim();
            Description = description?.Trim();
        }

    }
}
