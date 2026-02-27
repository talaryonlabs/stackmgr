using System.CommandLine;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager;

public class StackManagerCommand(string name, string description) : Command(name, description)
{
    protected string GetName<T>(ParseResult parseResult) where T : Symbol => parseResult.GetRequiredValue<string, T>().ToLower();
    
    protected StackEnvironment GetEnvironment<T>(ParseResult parseResult) where T : Symbol
    {
        var name = GetName<T>(parseResult);
        return StackEnvironment.Load(name);
    }
    
    protected Stack GetStack<T>(ParseResult parseResult, StackEnvironment env) 
        where T : Symbol
    {
        var name = GetName<T>(parseResult);
        return Stack.Load(env, name);
    }
    
    protected StackApp GetApp<T>(ParseResult parseResult, Stack stack) 
        where T : Symbol
    {
        var name = GetName<T>(parseResult);
        return stack.Apps.FirstOrDefault(v => v.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) ?? throw new AppNotFoundException(name);
    }
    
    protected StackIngress GetIngress<T>(ParseResult parseResult, Stack stack) 
        where T : Symbol
    {
        var hostname = GetName<T>(parseResult);
        return stack.Ingresses.FirstOrDefault(v => v.Hostname.Equals(hostname, StringComparison.CurrentCultureIgnoreCase)) ?? throw new IngressNotFoundException(hostname);
    }
    
    protected StackVolume GetVolume<T>(ParseResult parseResult, Stack stack) 
        where T : Symbol
    {
        var name = GetName<T>(parseResult);
        return stack.Volumes.FirstOrDefault(v => v.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) ?? throw new VolumeNotFoundException(name);
    }
    
    protected StackImage GetImage<T>(ParseResult parseResult, Stack stack) 
        where T : Symbol
    {
        var name = GetName<T>(parseResult);
        return stack.Images.FirstOrDefault(v => v.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) ?? throw new ImageNotFoundException(name);
    }
}