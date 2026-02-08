using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class AppCommand : StackManagerCommand
{
    public AppCommand() : base("app", "Manage applications")
    {
        var stackName = new StackArgument();
        var appName = new AppArgument();

        Add(new AppCreateCommand { stackName, appName });
        Add(new AppDeleteCommand { stackName, appName });
    }
}