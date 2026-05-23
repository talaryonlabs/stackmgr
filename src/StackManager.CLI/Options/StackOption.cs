using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class StackOption : Option<string>
{
    public StackOption() : base("--stack", "-s")
    {
        DefaultValueFactory = _ =>
        {
            var conf = LocalConfig.Get();
            return conf.Defaults.Stack ?? "default";
        };
        Description = "stack name (e.g. costumer1, project1)";
    }
}