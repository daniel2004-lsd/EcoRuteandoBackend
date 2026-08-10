using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Permissions.Queries.GetPermissionById
{
    public sealed record GetPermissionByIdResponse(
    Guid Id,
    string Name,
    string? Description);
}
