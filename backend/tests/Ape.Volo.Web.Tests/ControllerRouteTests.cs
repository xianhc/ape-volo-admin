using System.Reflection;
using Ape.Volo.Application.Contracts.Identity.Users;
using Ape.Volo.Application.Contracts.Identity.Auth;
using Ape.Volo.Application.Contracts.Identity;
using Ape.Volo.Application.Contracts.Operations;
using Ape.Volo.Application.Contracts.Message.Email.Accounts;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Web.Controllers.Auth;
using Ape.Volo.Web.DynamicApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Xunit;

namespace Ape.Volo.Web.Tests;

public class ControllerRouteTests
{
    [Fact]
    public void InternalReadContractsAndMethodsCannotBecomeHttpEndpoints()
    {
        var registry = DynamicApiRegistry.Build(typeof(IUserService).Assembly, new DynamicApiOptions());
        foreach (var contract in new[] { typeof(IUserLookupService), typeof(IAuthSessionService), typeof(IScheduledJobStore),
                     typeof(IEmailAccountLookupService), typeof(IPermissionCacheService), typeof(IDataScopeService),
                     typeof(Ape.Volo.Application.Contracts.Abstractions.IIdGenerator) })
            Assert.False(registry.IsExposed(contract));
        Assert.DoesNotContain(registry.GetEndpoints(typeof(IUserService)), endpoint =>
            endpoint.Method.Name is nameof(IUserService.GetByIdAsync) or nameof(IUserService.GetByNameAsync) or nameof(IUserService.UpdateAvatarAsync));
    }

    [Fact]
    public void EveryDynamicControllerRequiresAuthentication()
    {
        var registry = DynamicApiRegistry.Build(typeof(IUserService).Assembly, new DynamicApiOptions());
        var context = new ApplicationModelProviderContext(Array.Empty<System.Reflection.TypeInfo>());
        new DynamicApiApplicationModelProvider(registry).OnProvidersExecuting(context);

        Assert.NotEmpty(context.Result.Controllers);
        Assert.All(context.Result.Controllers, controller => Assert.Contains(controller.Filters, filter => filter is AuthorizeFilter));
    }

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
                "FileRecordController",
                "HealthCheckController",
                "OnlineUserController",
                "UserController"
            }, names);
    }
}
