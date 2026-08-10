using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Users.Queries.GetUsers
{
    public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string? LastName,
    string Email,
    string Role
);
}
