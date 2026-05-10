using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class ImageOption : Option<string[]>
{
    public ImageOption() : base("--image")
    {
        Description = "image (e.g. ghcr.io/org/repo:tag)";
        AllowMultipleArgumentsPerToken = true;
    }

    public static Dictionary<string, string> GetImages(ParseResult parseResult)
    {
        var value = parseResult.GetValue<string[], ImageOption>();
        if (value is null) return [];
        
        return value.Select(v =>
            {
                if (!v.Contains(':'))
                {
                    throw new ArgumentException("Volume must be in format '<name>:<value>'.");
                }

                var index = v.IndexOf(':');
                return new KeyValuePair<string, string>(v[..index], v[(index + 1)..]);
            })
            .ToDictionary();
    }
}
