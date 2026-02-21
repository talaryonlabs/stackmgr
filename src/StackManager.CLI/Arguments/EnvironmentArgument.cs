using System.CommandLine;

namespace Talaryon.StackManager.Arguments;

public class EnvironmentArgument : Argument<string>
{
    public EnvironmentArgument() : base("environment")
    {
        Description = "environment name (e.g. dev, prod)";
    }
}