using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Shared.Exceptions
{
    public sealed class UnauthorizedException : DomainException
    {
        public UnauthorizedException(string message)
            : base(message)
        {
        }
    }
}
