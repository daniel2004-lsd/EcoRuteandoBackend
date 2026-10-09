using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Roles.Commands.UpdateRole
{
    public sealed record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string? Description
) : IRequest;
}
