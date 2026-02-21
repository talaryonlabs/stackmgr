using System.CommandLine;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager;

public class StackManagerCommand(string name, string description) : Command(name, description)
{
    protected string GetEnvironmentName<T>(ParseResult parseResult) where T : Symbol => parseResult.GetRequiredValue<string, T>().ToLower();

    protected StackEnvironment GetEnvironment<T>(ParseResult parseResult) where T : Symbol
    {
        var name = GetEnvironmentName<T>(parseResult);
        return StackEnvironment.Load(name);
    }
    
    protected string GetStackName<T>(ParseResult parseResult) where T : Symbol => parseResult.GetRequiredValue<string, T>().ToLower();

    protected Stack GetStack<T>(ParseResult parseResult, StackEnvironment env) 
        where T : Symbol
    {
        var name = GetStackName<T>(parseResult);
        return Stack.Load(env, name);
    }
    
    protected string GetAppName<T>(ParseResult parseResult) where T : Symbol => parseResult.GetRequiredValue<string, T>().ToLower();
    
    protected StackApp GetApp<T>(ParseResult parseResult, Stack stack) 
        where T : Symbol
    {
        var name = GetAppName<T>(parseResult);
        return stack.Apps.FirstOrDefault(v => v.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) ?? throw new AppNotFoundException(name);
    }
}