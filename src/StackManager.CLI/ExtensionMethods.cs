using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager.Builder;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Services;
using Talaryon.Toolbox.Extensions;

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
            services.AddTransient<IAppService, AppService>();
            services.AddTransient<IGitService, GitService>();
            services.AddTransient<IKustomizeService, KustomizeService>();
            services.AddTransient<IProxyService, ProxyService>();
        
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

    extension(StackEnvironment env)
    {
        public Stack GetStack(string name)
        {
            var path = Path.Combine(env.LocalDirectory.FullName, name, Stack.FileName); 
            var file = new FileInfo(path);
        
            if (!file.Exists) throw new StackNotFoundException(name);

            var stack = StackConfig.Load<Stack>(file);

            stack.Environment = env;
            
            typeof(Stack)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(v => v.PropertyType.GetInterfaces().Any(i => i == typeof(IStackObject)))
                .Select(v => v.GetValue(stack))
                .OfType<IStackObject>()
                .ToList()
                .ForEach(v => v.Stack = stack);

            return stack;
        }
    }

    extension(Stack stack)
    {
        public IStackFactory<T> New<T>() where T : class, IStackObject
        {
            return new StackFactory<T>(stack);
        }

        public T Get<T>(string name) where T : class, IStackObject
        {
            var list = typeof(Stack)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(v => v.PropertyType == typeof(List<T>))
                .Select(v => v.GetValue(stack))
                .OfType<List<T>>()
                .FirstOrDefault();
            
            if(list is null)
                throw new InvalidOperationException();

            return list.FirstOrDefault(v => v.Name == name) ?? throw new ResourceNotFoundException<T>(stack, name);
        }
    }

    extension(StackApp app)
    {
        public async Task<bool> CheckRequirements(StackTemplate template)
        {
            var errors = new Dictionary<string, string>();
            var files = template.LocalDirectory
                .GetFileSystemInfos("*", SearchOption.AllDirectories);

            foreach (var requirement in app.Requirements.Where(requirement =>
                         !app.Stack.Apps.Exists(v => v.Name == requirement.Value)))
            {
                errors.TryAdd(requirement.Key, $"Required app '{requirement.Value}' not found in stack.");
            }

            foreach (var volume in app.Volumes.Where(volume => !app.Stack.Volumes.Exists(v => v.Name == volume.Value)))
            {
                errors.TryAdd(volume.Key, $"Required volume '{volume.Value}' not found in stack.");
            }

            foreach (var file in files)
            {
                var content = await File.ReadAllTextAsync(file.FullName);
                if (content.Contains("{{vault-path}}") && string.IsNullOrEmpty(app.Stack.Environment.Vault))
                {
                    errors.TryAdd("vault-path",
                        "Vault-Path is not configured. Please run 'stackmgr configure env <environment-name> --vault <vault-path>' first.");
                }
            }

            if (errors.Count == 0) return true;
            foreach (var error in errors)
            {
                LogMessage.AsError($"- {error.Value}");
            }

            return false;
        }
    }

    extension(IStackObject stackObject)
    {
        public void Delete<T>() where T : IStackObject
        {
            typeof(Stack)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(v => v.PropertyType == typeof(List<T>))
                .Select(v => v.GetValue(stackObject.Stack))
                .OfType<List<T>>()
                .ToList()
                .ForEach(v => v.Remove((T)stackObject));
            
            stackObject.Stack.SaveConfig();
        }
    }
}