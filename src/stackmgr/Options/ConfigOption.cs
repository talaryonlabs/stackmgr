using System.CommandLine;

namespace stackmgr.Options;

public class ConfigOption : Option<string[]>
{
    public ConfigOption() : base("--config")
    {
        AllowMultipleArgumentsPerToken = true;
    } 
}