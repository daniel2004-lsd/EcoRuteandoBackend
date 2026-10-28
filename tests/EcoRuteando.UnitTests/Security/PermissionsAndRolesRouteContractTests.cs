using EcoRuteando.Modules.Security.Presentation.Controllers;
using EcoRuteando.Shared.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;
using Xunit;

namespace EcoRuteando.UnitTests.Security;

public class PermissionsAndRolesRouteContractTests
{
    [Theory]
    [InlineData("GetPermissionById", typeof(HttpGetAttribute), "{id:guid}")]
    [InlineData("UpdatePermission", typeof(HttpPutAttribute), "{id:guid}")]
    [InlineData("DeletePermission", typeof(HttpDeleteAttribute), "{id:guid}")]
    public void PermissionsController_RouteConstraints_ShouldUseGuid(string methodName, Type httpAttrType, string expectedTemplate)
    {
        var method = typeof(PermissionsController).GetMethod(methodName);
        Assert.NotNull(method);

        var attr = method.GetCustomAttribute(httpAttrType) as HttpMethodAttribute;
        Assert.NotNull(attr);
        Assert.Equal(expectedTemplate, attr.Template);

        var idParam = method.GetParameters().FirstOrDefault(p => p.Name == "id");
        Assert.NotNull(idParam);
        Assert.Equal(typeof(Guid), idParam.ParameterType);
    }

    [Theory]
    [InlineData("GetRoleById", typeof(HttpGetAttribute), "{id:guid}")]
    [InlineData("UpdateRole", typeof(HttpPutAttribute), "{id:guid}")]
    [InlineData("DeleteRole", typeof(HttpDeleteAttribute), "{id:guid}")]
    public void RolesController_RouteConstraints_ShouldUseGuid(string methodName, Type httpAttrType, string expectedTemplate)
    {
        var method = typeof(RolesController).GetMethod(methodName);
        Assert.NotNull(method);

        var attr = method.GetCustomAttribute(httpAttrType) as HttpMethodAttribute;
        Assert.NotNull(attr);
        Assert.Equal(expectedTemplate, attr.Template);

        var idParam = method.GetParameters().FirstOrDefault(p => p.Name == "id");
        Assert.NotNull(idParam);
        Assert.Equal(typeof(Guid), idParam.ParameterType);
    }

    [Fact]
    public void PermissionsController_Endpoints_ShouldEnforceHasPermissionAttribute()
    {
        var methods = typeof(PermissionsController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach (var method in methods)
        {
            var hasPerm = method.GetCustomAttribute<HasPermissionAttribute>();
            Assert.True(hasPerm != null, $"Method {method.Name} in PermissionsController is missing [HasPermission]");
        }
    }

    [Fact]
    public void RolesController_Endpoints_ShouldEnforceHasPermissionAttribute()
    {
        var methods = typeof(RolesController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach (var method in methods)
        {
            var hasPerm = method.GetCustomAttribute<HasPermissionAttribute>();
            Assert.True(hasPerm != null, $"Method {method.Name} in RolesController is missing [HasPermission]");
        }
    }
}
