using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Roles.Queries.GetRoles
{
    public sealed record GetRolesQuery()
    : IRequest<IReadOnlyList<RoleResponse>>;
}
