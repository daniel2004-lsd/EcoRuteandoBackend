using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Presentation.Contracts.Auth
{
    public sealed record LoginRequest(
    string Email,
    string Password);
}
