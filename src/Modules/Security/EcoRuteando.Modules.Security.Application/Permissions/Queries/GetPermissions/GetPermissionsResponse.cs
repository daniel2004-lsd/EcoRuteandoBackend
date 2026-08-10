using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Permissions.Queries.GetPermissions
{
    public sealed record GetPermissionsResponse(
    Guid Id,
    string Name,
    string? Description);
}
