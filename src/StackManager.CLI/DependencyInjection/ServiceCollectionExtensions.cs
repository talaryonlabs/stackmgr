using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStackManagerServices(this IServiceCollection services)
    {
        // Register singleton services
        services.AddSingleton<LocalConfig>(_ => LocalConfig.Get());
        
        // Register transient services
        services.AddTransient<GitService>(sp => new GitService(sp.GetRequiredService<LocalConfig>()));
        services.AddTransient<AppService>();
        
        // Register named HttpClient for ProxyService
        services.AddHttpClient("ProxyService")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5
            });
        
        return services;
    }
    
    public static IServiceCollection AddStackManagerCommands(this IServiceCollection services)
    {
        // Register all command types for DI
        var commandTypes = typeof(ServiceCollectionExtensions).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && 
                       typeof(StackManagerCommand).IsAssignableFrom(t))
            .ToList();
        
        foreach (var commandType in commandTypes)
        {
            services.AddTransient(commandType);
        }
        
        return services;
    }
}
