using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class ConfigOption : Option<string[]>
{
    public ConfigOption() : base("--config")
    {
        AllowMultipleArgumentsPerToken = true;
    } 
}