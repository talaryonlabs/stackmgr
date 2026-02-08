using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class StackCommand : StackManagerCommand
{
    public StackCommand() : base("stack", "Manage stacks")
    {
        var stackName = new StackArgument();
        
        Add(new StackListCommand());
        Add(new StackNewCommand {stackName});
        Add(new StackDeleteCommand {stackName});
        Add(new StackDisableCommand {stackName});
        Add(new StackEnableCommand {stackName});
        Add(new StackBuildCommand {stackName});
        Add(new StackSyncCommand {stackName});
    }
}