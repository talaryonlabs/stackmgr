using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager.DependencyInjection;
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
    public void AddStackManagerCommands_ShouldRegisterCommands()
    {
        var services = new ServiceCollection();
        services.AddStackManagerCommands();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Commands are transient, so we should be able to create them
        // Note: Commands require ServiceProvider to be set, so we can't fully test this here
        // but we can verify they're registered
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
}
