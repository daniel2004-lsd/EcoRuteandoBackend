using EcoRuteando.Modules.Security.Application.Users.Queries.GetUsers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Users.Queries.GetUserById
{
    public sealed record GetUserByIdQuery(Guid UserId) : IRequest<UserResponse>;
}
