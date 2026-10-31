using System.Reflection;
using Ape.Volo.Web.Controllers.Auth;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Ape.Volo.Web.Tests;

public class ControllerRouteTests
{
    [Fact]
    public void AllControllersDeclareRoutes()
    {
        var controllers = typeof(AuthorizationController).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
            .Where(type => type.Namespace != "Ape.Volo.Web.Controllers.Base")
            .ToList();

        Assert.NotEmpty(controllers);
        Assert.All(controllers, controller =>
            Assert.True(controller.GetCustomAttribute<RouteAttribute>() != null, $"{controller.Name} is missing [Route]."));
    }

    [Fact]
    public void SpecialHttpContractsUseExplicitControllers()
    {
        var names = typeof(AuthorizationController).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
            .Where(type => type.Namespace != "Ape.Volo.Web.Controllers.Base")
            .Select(type => type.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "AuthorizationController",
                "ClientErrorController",
                "FileRecordController",
                "HealthCheckController",
                "OnlineUserController",
                "UserController"
            }, names);
    }
}
