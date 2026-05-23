using System.CommandLine;

namespace Talaryon.StackManager.Arguments;

public class AppArgument : Argument<string>
{
    public AppArgument() : base("app")
    {
        Description = "application name (e.g. web, project, test)";
    }
}