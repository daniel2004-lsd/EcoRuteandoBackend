using EcoRuteando.Shared.Exceptions;
using EcoRuteando.Shared.BaseClasses;
namespace EcoRuteando.Modules.Security.Domain.Entities;


public sealed class Role : Entity<int>
{
    public string Name { get; private set; } = string.Empty;
    public ICollection<RolePermission> RolePermissions { get; private set; } = [];
    public ICollection<UserRole> UserRoles { get; private set; } = [];

    public string? Description { get; private set; }
    public ICollection<Permission> permissions { get; private set; } = [];


    private Role()
    {
    }

    public Role(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del rol es obligatorio.");

        Name = name.Trim();
        Description = description;
    }
}