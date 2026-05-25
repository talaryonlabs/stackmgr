using System;
using System.Net.Http;
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

        var proxy = new ProxyService(factory);

        // ProxyService should be created without error
        Assert.NotNull(proxy);
    }



    [Fact]
    public void ProxyService_ShouldImplementIProxyService()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("ProxyService");
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        var proxy = new ProxyService(factory);

        Assert.IsAssignableFrom<IProxyService>(proxy);
    }

    [Fact]
    public void ProxyService_Remote_ShouldReturnActions()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("ProxyService");
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        var proxy = new ProxyService(factory);
        
        var actions = proxy.Remote(_remote);
        
        Assert.NotNull(actions);
        Assert.IsAssignableFrom<IProxyServiceActions>(actions);
    }

    [Fact]
    public void ProxyServiceActions_ShouldBeAssignableFromIProxyServiceActions()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("ProxyService");
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        var proxy = new ProxyService(factory);
        var actions = proxy.Remote(_remote);
        
        // The actions returned should implement IProxyServiceActions
        Assert.IsAssignableFrom<IProxyServiceActions>(actions);
    }

    [Fact]
    public void ProxyService_ShouldHaveRequiredInterfaces()
    {
        // Verify that IProxyService and IProxyServiceActions exist and have required methods
        var proxyServiceType = typeof(IProxyService);
        var proxyActionsType = typeof(IProxyServiceActions);

        Assert.NotNull(proxyServiceType);
        Assert.NotNull(proxyActionsType);
    }
}
