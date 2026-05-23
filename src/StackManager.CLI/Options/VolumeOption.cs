using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class VolumeOption : Option<string[]>
{
    public VolumeOption() : base("--volume")
    {
        Description = "volume";
        AllowMultipleArgumentsPerToken = true;
    }

    public static Dictionary<string, string> GetVolumes(ParseResult parseResult)
    {
        var value = parseResult.GetValue<string[], VolumeOption>();
        if (value is null) return [];
        
        return value.Select(v =>
            {
                if (!v.Contains(':'))
                {
                    throw new ArgumentException("Volume must be in format 'volume:<name>'.");
                }

                var index = v.IndexOf(':');
                return new KeyValuePair<string, string>(v[..index], v[(index + 1)..]);
            })
            .ToDictionary();
    }
}