using System.CommandLine;
using stackmgr.Options;

namespace stackmgr;

public static class ExtensionMethods
{
    public static TValue GetRequiredValue<TValue, TSymbol>(this ParseResult parseResult) where TSymbol : Symbol 
    {
        var item = Activator.CreateInstance<TSymbol>();
        return parseResult.GetRequiredValue<TValue>(item.Name);
    }
    
    public static TValue? GetValue<TValue, TSymbol>(this ParseResult parseResult) where TSymbol : Symbol 
    {
        var item = Activator.CreateInstance<TSymbol>();
        return parseResult.GetValue<TValue>(item.Name);
    }

    public static string GetStackPath(this StackEnvironment environment, string stackName)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), environment.Name.ToLower(), stackName);
    }
    
    public static bool HasStack(this StackEnvironment environment, string stackName)
    {
        return Directory.Exists(GetStackPath(environment, stackName));
    }
}