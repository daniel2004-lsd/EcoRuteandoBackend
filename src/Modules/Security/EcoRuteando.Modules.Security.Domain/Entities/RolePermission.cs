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
        public int RoleId { get; private set; } // Foreign key to Role

        public int PermissionId { get; private set; } // Foreign key to Permission

        public Role Role { get; private set; } = null!;

        public Permission Permission { get; private set; } = null!;

        private RolePermission()
        {
        }

        public RolePermission(int roleId, int permissionId)
        {
            RoleId = roleId;
            PermissionId = permissionId;
        }
    }
}
