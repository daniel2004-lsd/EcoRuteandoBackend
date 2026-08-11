using EcoRuteando.Shared.BaseClasses;
using EcoRuteando.Shared.Exceptions;

namespace EcoRuteando.Modules.Security.Domain.Entities;

public sealed class Role : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = [];

    public ICollection<UserRole> UserRoles { get; private set; } = [];

    private Role()
    {
    }

    public Role(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del rol es obligatorio.");

        Name = name.Trim();
        Description = description?.Trim();
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del rol es obligatorio.");

        Name = name.Trim();
        Description = description?.Trim();
    }
}