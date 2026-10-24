using System.Reflection;
using EcoRuteando.Modules.Admin.Presentation.Controllers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace EcoRuteando.UnitTests.Admin.Controllers;

public class AdminAuthorizationTests
{
    private const string ExpectedRoles = "Admin,SuperAdmin";

    [Fact]
    public void ExportsController_ShouldRequireAdminOrSuperAdminAtClassLevel()
    {
        var authAttr = typeof(ExportsController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authAttr);
        Assert.Equal(ExpectedRoles, authAttr.Roles);
    }

    [Fact]
    public void StatisticsController_ShouldRequireAdminOrSuperAdminAtClassLevel()
    {
        var authAttr = typeof(StatisticsController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authAttr);
        Assert.Equal(ExpectedRoles, authAttr.Roles);
    }

    [Fact]
    public void SystemConfigurationController_UpdateConfiguration_ShouldRequireAdminOrSuperAdmin()
    {
        var method = typeof(SystemConfigurationController).GetMethod("UpdateConfiguration");
        Assert.NotNull(method);
        var authAttr = method.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authAttr);
        Assert.Equal(ExpectedRoles, authAttr.Roles);
    }

    [Fact]
    public void SystemConfigurationController_GetConfiguration_ShouldAllowAnonymous()
    {
        var method = typeof(SystemConfigurationController).GetMethod("GetConfiguration");
        Assert.NotNull(method);
        var anonAttr = method.GetCustomAttribute<AllowAnonymousAttribute>();
        Assert.NotNull(anonAttr);
    }

    [Theory]
    [InlineData("GetAll")]
    [InlineData("Create")]
    [InlineData("Update")]
    public void TransportFactorsController_AdminEndpoints_ShouldRequireAdminOrSuperAdmin(string methodName)
    {
        var method = typeof(TransportFactorsController).GetMethod(methodName);
        Assert.NotNull(method);
        var authAttr = method.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authAttr);
        Assert.Equal(ExpectedRoles, authAttr.Roles);
    }

    [Fact]
    public void TransportFactorsController_GetActive_ShouldAllowAnonymous()
    {
        var method = typeof(TransportFactorsController).GetMethod("GetActive");
        Assert.NotNull(method);
        var anonAttr = method.GetCustomAttribute<AllowAnonymousAttribute>();
        Assert.NotNull(anonAttr);
    }
}
