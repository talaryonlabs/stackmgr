using System.CommandLine;

namespace stackmgr.Arguments;

public class EnvironmentArgument : Argument<string>
{
    public EnvironmentArgument() : base("environment")
    {
        Description = "environment name (e.g. dev, prod)";
    }
}