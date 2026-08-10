using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Presentation.Contracts.Role
{
    public sealed record CreateRoleRequest(
    string Name,
    string? Description);
}
