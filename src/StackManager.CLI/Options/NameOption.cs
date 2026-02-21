using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class NameOption : Option<string>
{
    public NameOption() : base("--name")
    {
        Description = "image name (e.g. nginx)";
    }
}