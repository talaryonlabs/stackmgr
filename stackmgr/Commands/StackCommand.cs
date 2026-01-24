using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class StackCommand : Command
{
    public StackCommand() : base("stack", "Manage stacks")
    {
        var stackName = new StackNameArgument();
        
        Add(new EnvironmentOption { Required = true, Recursive = true });
        Add(new StackListCommand());
        Add(new StackNewCommand {stackName});
        Add(new StackDeleteCommand {stackName});
        Add(new StackDisableCommand {stackName});
        Add(new StackEnableCommand {stackName});
        Add(new StackBuildCommand {stackName});
    }
}