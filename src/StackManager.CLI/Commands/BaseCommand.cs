using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands;

public class BaseCommand(string name, string description) : Command(name, description)
{
    private IServiceProvider? _serviceProvider;
    
    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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
        return Stack.Load(env, name);
    }
    
    protected static StackApp GetApp<T>(ParseResult parseResult, Stack stack) 
        where T : Symbol
    {
        var name = GetName<T>(parseResult);
        return stack.Apps.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? throw new AppNotFoundException(name);
    }
    
    protected static StackIngress GetIngress<T>(ParseResult parseResult, Stack stack) 
        where T : Symbol
    {
        var hostname = GetName<T>(parseResult);
        return stack.Ingresses.FirstOrDefault(v => v.Hostname.Equals(hostname, StringComparison.OrdinalIgnoreCase)) ?? throw new IngressNotFoundException(hostname);
    }
    
    protected static StackVolume GetVolume<T>(ParseResult parseResult, Stack stack) 
        where T : Symbol
    {
        var name = GetName<T>(parseResult);
        return stack.Volumes.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? throw new VolumeNotFoundException(name);
    }
    
    protected static StackImage GetImage<T>(ParseResult parseResult, Stack stack) 
        where T : Symbol
    {
        var name = GetName<T>(parseResult);
        return stack.Images.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? throw new ImageNotFoundException(name);
    }
}