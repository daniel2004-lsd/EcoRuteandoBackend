using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Permissions.Commands.UpdatePermission
{
    public sealed record UpdatePermissionCommand(
     int PermissionId,
     string Name,
     string? Description)
     : IRequest;
}
