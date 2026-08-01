using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Roles.Command.UpdateRole
{
    public sealed record UpdateRoleCommand(
    int RoleId,
    string Name,
    string? Description
) : IRequest;
}
