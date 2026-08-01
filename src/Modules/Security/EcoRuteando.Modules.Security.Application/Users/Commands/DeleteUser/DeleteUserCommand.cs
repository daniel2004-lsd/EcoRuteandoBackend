using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace EcoRuteando.Modules.Security.Application.Users.Commands.DeleteUser
{


    public sealed record DeleteUserCommand(Guid UserId) : IRequest;
}
