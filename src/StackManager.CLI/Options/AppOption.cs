using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class AppOption : Option<string>
{
    public AppOption() : base("--app", "-a")
    {
        Description = "application name";
    }
}