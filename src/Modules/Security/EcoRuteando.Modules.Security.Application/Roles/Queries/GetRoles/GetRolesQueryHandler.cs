using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRuteando.Modules.Security.Application.Roles.Queries.GetRoles
{
    public sealed class GetRolesQueryHandler
    : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleResponse>>
    {
        private readonly IRoleRepository _repository;

        public GetRolesQueryHandler(IRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<RoleResponse>> Handle(
            GetRolesQuery request,
            CancellationToken cancellationToken)
        {
            var roles = await _repository.GetAllAsync(cancellationToken);

            return roles
                .Select(role => new RoleResponse(
                    role.Id,
                    role.Name,
                    role.Description))
                .ToList();
        }
    }
}
