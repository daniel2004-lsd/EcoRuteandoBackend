using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.commands.RegisterUser
{
    public sealed record RegisterUserCommandValidator // estamos usando un record para que sea inmutable y no se pueda modificar
    (
        string FirstName,
        string LastName,
        string Email,
        string Password
    ) : IRequest<Guid>;
}
