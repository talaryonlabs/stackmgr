using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class EnvironmentOption : Option<string>
{
    public EnvironmentOption() : base("--environment", "--env")
    {
        DefaultValueFactory = _ =>
        {
            var conf = LocalConfig.Get();
            return conf.Default.Environment ?? "default";
        };
        Description = "environment name (e.g. dev, prod)";
    }
}