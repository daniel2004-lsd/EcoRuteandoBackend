using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Presentation.Contracts.Auth
{
    public sealed record RegisterRequest(
    string FirstName,
    string? LastName,
    string Email,
    string Password);
}
