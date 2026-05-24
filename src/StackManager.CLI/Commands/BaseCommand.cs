using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands;

public class BaseCommand(string name, string description) : Command(name, description)
{
    private IServiceProvider? _serviceProvider;

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        Children.OfType<BaseCommand>()
            .ToList()
            .ForEach(v => v.SetServiceProvider(serviceProvider));
        
        _serviceProvider = serviceProvider;
    }

    protected void UseAutodiscoverCommands(Type type)
    {
        // Auto-discover and add all type implementations
        var commands = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(v => v.BaseType?.IsGenericType == true 
                        && v.BaseType.GetGenericTypeDefinition() == type
                        && !v.IsAbstract)
            .Select(v => (BaseCommand?)Activator.CreateInstance(v))
            .OfType<BaseCommand>()
            .ToList();

        foreach (var instance in commands)
        {
            Add(instance);
        }
    }
    
    protected T GetRequiredService<T>() where T : class
    {
        return _serviceProvider == null
            ? throw new InvalidOperationException("Service provider not configured. Call SetServiceProvider first.")
            : _serviceProvider.GetRequiredService<T>();
    }
    
    protected T? GetService<T>() where T : class
    {
        return _serviceProvider?.GetService<T>();
    }
    
    protected static string GetName<T>(ParseResult parseResult) where T : Symbol => parseResult.GetRequiredValue<string, T>().ToLower();
    
    protected static StackEnvironment GetEnvironment<T>(ParseResult parseResult) where T : Symbol
    {
        var name = GetName<T>(parseResult);
        return StackEnvironment.Load(name);
    }
    
    protected static Stack GetStack<T>(ParseResult parseResult, StackEnvironment env) 
        where T : Symbol
    {
        var name = GetName<T>(parseResult);
        return env.GetStack(name);
    }
    
    protected static StackApp GetApp<T>(ParseResult parseResult, Stack stack) 
        where T : Symbol
    {
        var name = GetName<T>(parseResult);
        return stack.Apps.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? throw new AppNotFoundException(name);
    }
}