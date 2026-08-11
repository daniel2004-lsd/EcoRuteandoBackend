using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Roles.Queries.GetRoles
{
    public sealed record RoleResponse(
    int Id,
    string Name,
    string? Description);
}
