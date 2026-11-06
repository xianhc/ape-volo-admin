using Ape.Volo.Application.Identity.Roles;
using Xunit;

namespace Ape.Volo.Application.Tests;

public class ServiceBasicTests
{
    [Fact]
    public void AllServicesImplementMatchingInterfaces()
    {
        var services = typeof(RoleService).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.Name.EndsWith("Service"))
            .ToList();
        Assert.NotEmpty(services);
        Assert.All(services, service =>
            Assert.Contains(service.GetInterfaces(), contract => contract.Name == "I" + service.Name));
    }
}
