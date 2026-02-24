using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class StackOption : Option<string>
{
    public StackOption() : base("--stack")
    {
        DefaultValueFactory = _ => Environment.GetEnvironmentVariable("STACKMGR_STACK", EnvironmentVariableTarget.User) ?? "default";
        Description = "stack name (e.g. costumer1, project1)";
    }
}