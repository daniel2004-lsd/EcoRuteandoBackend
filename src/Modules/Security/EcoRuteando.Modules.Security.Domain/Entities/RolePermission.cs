using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcoRuteando.Modules.Security.Domain.Entities;


namespace EcoRuteando.Modules.Security.Domain.Entities
{
    public sealed class RolePermission

    {
        public Guid RoleId { get; private set; } // Foreign key to Role

        public Guid PermissionId { get; private set; } // Foreign key to Permission

        public Role Role { get; private set; } = null!;

        public Permission Permission { get; private set; } = null!;

        private RolePermission()
        {
        }

        public RolePermission(Guid roleId, Guid permissionId)
        {
            RoleId = roleId;
            PermissionId = permissionId;
        }
    }
}
