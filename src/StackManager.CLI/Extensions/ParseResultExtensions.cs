using System.CommandLine;

namespace Talaryon.StackManager.Extensions;

public static class ParseResultExtensions
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
}