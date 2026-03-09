using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class RequirementOption : Option<string[]>
{
    public RequirementOption() : base("--requirement")
    {
        Description = "";
        AllowMultipleArgumentsPerToken = true;
    }

    public static Dictionary<string, string> GetRequirements(ParseResult parseResult)
    {
        var value = parseResult.GetValue<string[], RequirementOption>();
        if (value is null) return [];
        
        return value.Select(v =>
            {
                if (!v.Contains(':'))
                {
                    throw new ArgumentException("Requirement must be in format 'requirement:<name>'.");
                }

                var index = v.IndexOf(':');
                return new KeyValuePair<string, string>(v[..index], v[(index + 1)..]);
            })
            .ToDictionary();
    }
}