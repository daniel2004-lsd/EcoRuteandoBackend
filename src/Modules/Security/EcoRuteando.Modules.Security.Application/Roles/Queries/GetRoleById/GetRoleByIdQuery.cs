using EcoRuteando.Modules.Security.Application.Roles.Queries.GetRoles;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Roles.Queries.GetRoleById
{
    public sealed record GetRoleByIdQuery(int RoleId)
    : IRequest<RoleResponse>;
}
