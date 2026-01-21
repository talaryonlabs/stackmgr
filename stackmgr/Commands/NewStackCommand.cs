using System.CommandLine;
using stackmgr.Arguments;

namespace stackmgr.Commands;

public class NewStackCommand : Command
{
    public NewStackCommand() : base("new-stack", "Create a new stack")
    {
        Add(new StackNameArgument());
    }
}