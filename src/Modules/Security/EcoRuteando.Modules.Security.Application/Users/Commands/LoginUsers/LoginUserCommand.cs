using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.LoginUsers
{
    public sealed record LoginUserCommand
    (
        string Email,
        string Password
    ) : IRequest<LoginResponse>;
}
