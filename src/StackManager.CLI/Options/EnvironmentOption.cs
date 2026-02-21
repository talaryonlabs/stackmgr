using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class EnvironmentOption : Option<string>
{
    public EnvironmentOption() : base("--environment", "--env")
    {
        DefaultValueFactory = _ => Environment.GetEnvironmentVariable("STACKMGR_ENV", EnvironmentVariableTarget.User) ?? "default";
        Description = "environment name (e.g. dev, prod)";
    }
}