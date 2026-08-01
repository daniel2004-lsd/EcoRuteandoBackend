using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Abstractions.Security
{
    public interface IJwtProvider
    {
        string GenerateToken(
            Guid userId,
            string email,
            string role);
    }
}
