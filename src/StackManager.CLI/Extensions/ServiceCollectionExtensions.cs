using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager.Builder;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddStackManagerServices()
        {
            // Register singleton services
            services.AddSingleton<LocalConfig>(_ => LocalConfig.Get());
        
            // Register transient services
            services.AddTransient<IGitService, GitService>();
            services.AddTransient<ITemplateService, TemplateService>();
            services.AddTransient<IKustomizeService, KustomizeService>();
            services.AddTransient<IProxyService, ProxyService>();
            services.AddTransient<ISyncService, SyncService>();
        
            // Register named HttpClient for ProxyService
            services.AddHttpClient("ProxyService")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    MaxAutomaticRedirections = 5
                });
        
            return services;
        }
    }
}