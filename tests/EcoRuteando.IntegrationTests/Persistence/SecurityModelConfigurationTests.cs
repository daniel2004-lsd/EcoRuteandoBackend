using EcoRuteando.Modules.Security.Domain.Entities;
using EcoRuteando.Modules.Security.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace EcoRuteando.IntegrationTests.Persistence;

public sealed class SecurityModelConfigurationTests
{
    private SecurityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SecurityDbContext>()
            .UseNpgsql("Host=localhost;Database=dummy;Username=dummy;Password=dummy")
            .Options;

        return new SecurityDbContext(options);
    }

    [Fact]
    public void UserModel_ShouldMapToSecuritySchemaAndUsersTable()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));

        Assert.NotNull(entityType);
        Assert.Equal("users", entityType.GetTableName());
        Assert.Equal("security", entityType.GetSchema());
    }

    [Fact]
    public void UserModel_ShouldHaveUniqueIndexOnEmail()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));

        var emailProp = entityType?.FindProperty(nameof(User.Email));
        Assert.NotNull(emailProp);
        Assert.Equal("email", emailProp.GetColumnName());
        Assert.Equal("citext", emailProp.GetColumnType());
        Assert.False(emailProp.IsNullable);

        var indexes = entityType!.GetIndexes();
        var emailIndex = indexes.FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == nameof(User.Email)));

        Assert.NotNull(emailIndex);
        Assert.True(emailIndex.IsUnique);
    }

    [Fact]
    public void UserModel_ShouldEnforceExpectedMaxLengths()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));

        Assert.NotNull(entityType);
        Assert.Equal(100, entityType.FindProperty(nameof(User.FirstName))?.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty(nameof(User.LastName))?.GetMaxLength());
        Assert.Equal(255, entityType.FindProperty(nameof(User.PasswordHash))?.GetMaxLength());
        Assert.Equal(30, entityType.FindProperty(nameof(User.PhoneNumber))?.GetMaxLength());
        Assert.Equal(500, entityType.FindProperty(nameof(User.ProfilePhotoUrl))?.GetMaxLength());
        Assert.Equal(7, entityType.FindProperty(nameof(User.PrimaryColor))?.GetMaxLength());
    }

    [Fact]
    public void UserModel_ShouldHaveConfiguredCheckConstraints()
    {
        using var context = CreateContext();
        var designModel = context.GetService<IDesignTimeModel>().Model;
        var entityType = designModel.FindEntityType(typeof(User));

        Assert.NotNull(entityType);
        var checkConstraints = entityType.GetCheckConstraints().Select(c => c.Name).ToList();

        Assert.Contains("chk_users_status", checkConstraints);
        Assert.Contains("chk_users_preferred_language", checkConstraints);
        Assert.Contains("chk_users_ui_theme", checkConstraints);
        Assert.Contains("users_phone_number_check", checkConstraints);
        Assert.Contains("users_primary_color_check", checkConstraints);
    }

    [Fact]
    public void UserRoleModel_ShouldHaveCompositePrimaryKey()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(UserRole));

        Assert.NotNull(entityType);
        var key = entityType.FindPrimaryKey();

        Assert.NotNull(key);
        var keyPropNames = key.Properties.Select(p => p.Name).ToList();
        Assert.Contains(nameof(UserRole.UserId), keyPropNames);
        Assert.Contains(nameof(UserRole.RoleId), keyPropNames);
    }
}
