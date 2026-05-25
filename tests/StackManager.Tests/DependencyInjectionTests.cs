using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager.Extensions;
using Talaryon.StackManager.Services;
using Xunit;

namespace Talaryon.StackManager.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddStackManagerServices_ShouldRegisterLocalConfigAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddStackManagerServices();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Should be able to resolve LocalConfig as singleton
        var config1 = serviceProvider.GetService<LocalConfig>();
        var config2 = serviceProvider.GetService<LocalConfig>();
        
        Assert.NotNull(config1);
        Assert.Same(config1, config2); // Should be singleton
    }

    [Fact]
    public void AddStackManagerServices_ShouldRegisterHttpClientFactory()
    {
        var services = new ServiceCollection();
        services.AddStackManagerServices();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Should have HttpClientFactory
        var factory = serviceProvider.GetService<IHttpClientFactory>();
        Assert.NotNull(factory);
        
        // Should be able to create named client
        var client = factory.CreateClient("ProxyService");
        Assert.NotNull(client);
    }

    [Fact]
    public void ServiceCollectionExtensions_ShouldRegisterHttpClient()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddStackManagerServices();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Should be able to resolve IHttpClientFactory
        var factory = serviceProvider.GetService<IHttpClientFactory>();
        Assert.NotNull(factory);
    }

    [Fact]
    public void AddStackManagerServices_ShouldRegisterIProxyService()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddStackManagerServices();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Should be able to resolve IProxyService
        var proxyService = serviceProvider.GetService<IProxyService>();
        Assert.NotNull(proxyService);
    }

    [Fact]
    public void ServiceProvider_ShouldCreateProxyServiceActions()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddStackManagerServices();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Should be able to resolve IProxyService and call Remote
        var proxyService = serviceProvider.GetService<IProxyService>();
        Assert.NotNull(proxyService);
        
        var remote = new LocalConfigRemote
        {
            Name = "test-remote",
            Url = "https://api.example.com",
            AccessToken = "test-token"
        };
        
        var actions = proxyService.Remote(remote);
        Assert.NotNull(actions);
        Assert.IsAssignableFrom<IProxyServiceActions>(actions);
    }
}
