using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Roles.Command.CreateRole
{
    public sealed record CreateRoleCommand(
    string Name,
    string? Description
) : IRequest<int>;
}
