using System.CommandLine;
using stackmgr.Exceptions;

namespace stackmgr;

public class StackManagerCommand(string name, string description) : Command(name, description)
{
    protected static readonly StackManagerConfig Config;
    
    static StackManagerCommand()
    {
        Config = StackManagerConfig.Load();
    }

    protected string GetEnvironmentName<T>(ParseResult parseResult) where T : Symbol => parseResult.GetRequiredValue<string, T>().ToLower();

    protected StackEnvironment GetEnvironment<T>(ParseResult parseResult) where T : Symbol
    {
        var name = GetEnvironmentName<T>(parseResult);
        var env =
            Config.Environments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        return env ?? throw new EnvironmentNotFoundException(name);
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
        throw null;
    }
}