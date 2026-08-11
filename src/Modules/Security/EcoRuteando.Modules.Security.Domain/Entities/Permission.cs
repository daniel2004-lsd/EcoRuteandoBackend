using EcoRuteando.Shared.BaseClasses;
using EcoRuteando.Shared.Exceptions;

namespace EcoRuteando.Modules.Security.Domain.Entities;

public sealed class Permission : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = [];

    private Permission()
    {
    }

    public Permission(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(
                "El nombre del permiso es obligatorio.");

        Name = name.Trim();
        Description = description?.Trim();
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(
                "El nombre del permiso es obligatorio.");

        Name = name.Trim();
        Description = description?.Trim();
    }
}