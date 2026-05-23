using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager;

public static class ExtensionMethods
{
    extension(ParseResult parseResult)
    {
        public TValue GetRequiredValue<TValue, TSymbol>() where TSymbol : Symbol 
        {
            var item = Activator.CreateInstance<TSymbol>();
            return parseResult.GetRequiredValue<TValue>(item.Name);
        }

        public TValue? GetValue<TValue, TSymbol>() where TSymbol : Symbol 
        {
            var item = Activator.CreateInstance<TSymbol>();
            return parseResult.GetValue<TValue>(item.Name);
        }
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection AddStackManagerServices()
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
    
        public IServiceCollection AddStackManagerCommands()
        {
            // Register all command types for DI
            var commandTypes = typeof(ExtensionMethods).Assembly
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
}