using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Presentation.Contracts.Permission
{
    public sealed record CreatePermissionRequest(
    string Name,
    string? Description);
}
