using System.CommandLine;

namespace stackmgr.Options;

public class EnvironmentOption : Option<string>
{
    public EnvironmentOption() : base("--environment", "--env")
    {
        DefaultValueFactory = _ => "default";
        Description = "environment name (e.g. dev, prod)";
    }
}