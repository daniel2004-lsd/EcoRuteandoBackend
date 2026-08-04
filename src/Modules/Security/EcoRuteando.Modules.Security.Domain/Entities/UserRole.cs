using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Domain.Entities
{
    public sealed class UserRole

    {
        public Guid UserId { get; private set; } // Foreign key to User

        public Guid RoleId { get; private set; } // Foreign key to Role


        public User User { get; private set; } = null!;

        public Role Role { get; private set; } = null!;


        public UserRole() { }

        public UserRole(Guid userId, Guid roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
}
