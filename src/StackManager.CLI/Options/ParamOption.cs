using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class ParamOption : Option<string[]>
{
    public ParamOption() : base("--param")
    {
        Description = "parameter (e.g. hostname:hallo)";
        AllowMultipleArgumentsPerToken = true;
    }
    
    public static Dictionary<string, string> GetParams(ParseResult parseResult)
    {
        var value = parseResult.GetValue<string[], ParamOption>();
        if (value is null) return [];
        
        return value.Select(v =>
            {
                if (!v.Contains(':'))
                {
                    throw new ArgumentException("Parameter must be in format 'parameter:<name>'.");
                }

                var index = v.IndexOf(':');
                return new KeyValuePair<string, string>(v[..index], v[(index + 1)..]);
            })
            .ToDictionary();
    }
}