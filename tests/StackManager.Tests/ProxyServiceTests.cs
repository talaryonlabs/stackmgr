using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager.Services;
using Xunit;

namespace Talaryon.StackManager.Tests;

public class ProxyServiceTests
{
    private readonly LocalConfigRemote _remote = new()
    {
        Name = "test-remote",
        Url = "https://api.example.com",
        AccessToken = "test-token"
    };

    [Fact]
    public void ProxyService_Constructor_ShouldInitializeClient()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("ProxyService");
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        var proxy = new ProxyService(_remote, factory);

        // ProxyService should be created without error
        Assert.NotNull(proxy);
    }

    [Fact]
    public void ProxyService_Dispose_ShouldNotThrow()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("ProxyService");
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        var proxy = new ProxyService(_remote, factory);

        // Should not throw
        proxy.Dispose();
    }

    [Fact]
    public void ProxyService_ShouldImplementIProxyService()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("ProxyService");
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        var proxy = new ProxyService(_remote, factory);

        Assert.IsAssignableFrom<IProxyService>(proxy);
    }


}
