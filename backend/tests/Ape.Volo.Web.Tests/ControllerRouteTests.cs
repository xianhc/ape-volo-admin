using System.Reflection;
using Ape.Volo.Application.Contracts.Identity.Users;
using Ape.Volo.Application.Contracts.Identity.Auth;
using Ape.Volo.Application.Contracts.Identity;
using Ape.Volo.Application.Contracts.System.Jobs;
using Ape.Volo.Application.Contracts.Log;
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
    [Theory]
    [InlineData(typeof(IJobService), "GetListAsync", "GET", "api/job/get-list")]
    [InlineData(typeof(IJobService), "GetAllAsync", "GET", "api/job/get-all")]
    [InlineData(typeof(IJobService), "DownloadAsync", "POST", "api/job/download")]
    [InlineData(typeof(IJobManager), "CreateAsync", "POST", "api/job-manager/create")]
    [InlineData(typeof(IJobManager), "UpdateAsync", "PUT", "api/job-manager/update")]
    [InlineData(typeof(IJobManager), "DeleteAsync", "DELETE", "api/job-manager/delete")]
    [InlineData(typeof(IJobManager), "ExecuteAsync", "POST", "api/job-manager/execute")]
    [InlineData(typeof(IJobManager), "PauseAsync", "POST", "api/job-manager/pause")]
    [InlineData(typeof(IJobManager), "ResumeAsync", "POST", "api/job-manager/resume")]
    [InlineData(typeof(IJobManager), "StopAsync", "POST", "api/job-manager/stop")]
    [InlineData(typeof(IJobManager), "RestartAsync", "POST", "api/job-manager/restart")]
    [InlineData(typeof(IJobLogService), "GetListAsync", "GET", "api/job-log/get-list")]
    [InlineData(typeof(IJobLogService), "DownloadAsync", "POST", "api/job-log/download")]
    public void JobEndpointsMatchClientRoutes(Type contract, string method, string verb, string route)
    {
        var registry = DynamicApiRegistry.Build(contract.Assembly, new DynamicApiOptions());
        var endpoint = Assert.Single(registry.GetEndpoints(contract), endpoint => endpoint.Method.Name == method);
        Assert.Equal(verb, endpoint.HttpMethod);
        Assert.Equal(route, endpoint.RouteTemplate);
        Assert.DoesNotContain(registry.All, endpoint => endpoint.RouteTemplate.Contains("scheduled-job"));
    }

    [Fact]
    public void InternalReadContractsAndMethodsCannotBecomeHttpEndpoints()
    {
        var registry = DynamicApiRegistry.Build(typeof(IUserService).Assembly, new DynamicApiOptions());
        foreach (var contract in new[] { typeof(IUserLookupService), typeof(IAuthSessionService), typeof(IJobStore),
                     typeof(IEmailAccountLookupService), typeof(IPermissionCacheService), typeof(IDataScopeService),
                     typeof(Ape.Volo.Application.Contracts.Abstractions.IIdGenerator) })
            Assert.False(registry.IsExposed(contract));
        Assert.DoesNotContain(registry.GetEndpoints(typeof(IUserService)), endpoint =>
            endpoint.Method.Name is nameof(IUserService.GetByIdAsync) or nameof(IUserService.GetByNameAsync) or nameof(IUserService.UpdateAvatarAsync));
        Assert.DoesNotContain(registry.GetEndpoints(typeof(IJobService)), endpoint =>
            endpoint.Method.Name is nameof(IJobService.CreateAsync) or nameof(IJobService.UpdateAsync)
                or nameof(IJobService.DeleteAsync) or nameof(IJobService.UpdateJobInfoAsync));
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
