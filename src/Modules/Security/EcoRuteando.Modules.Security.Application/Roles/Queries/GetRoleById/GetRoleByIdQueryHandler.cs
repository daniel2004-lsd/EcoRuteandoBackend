using EcoRuteando.Modules.Security.Application.Roles.Queries.GetRoleById;
using EcoRuteando.Modules.Security.Application.Roles.Queries.GetRoles;
using EcoRuteando.Shared.Exceptions;
using MediatR;

public sealed class GetRoleByIdQueryHandler
    : IRequestHandler<GetRoleByIdQuery, RoleResponse>
{
    private readonly IRoleRepository _repository;

    public GetRoleByIdQueryHandler(IRoleRepository repository)
    {
        _repository = repository;
    }

    public async Task<RoleResponse> Handle(
        GetRoleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var role = await _repository.GetByIdAsync(
            request.RoleId,
            cancellationToken);

        if (role is null)
        {
            throw new NotFoundException("El rol no existe.");
        }

        return new RoleResponse(
            role.Id,
            role.Name,
            role.Description);
    }
}