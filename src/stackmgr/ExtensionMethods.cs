using System.CommandLine;

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

    extension(StackEnvironment environment)
    {
        public string GetStackPath(string stackName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), environment.Name.ToLower(), stackName);
        }

        public string GetStackNamespace(string stackName)
        {
            return $"{environment.Name.ToLower()}-{stackName}";
        }

        public bool HasLocalStack(string stackName)
        {
            return Directory.Exists(environment.GetStackPath(stackName));
        }
    }
}