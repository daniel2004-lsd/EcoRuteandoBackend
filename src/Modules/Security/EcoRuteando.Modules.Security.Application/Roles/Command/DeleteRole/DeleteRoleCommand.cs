using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Roles.Commands.DeleteRole
{
    public sealed record DeleteRoleCommand(
    Guid RoleId
) : IRequest;
}
