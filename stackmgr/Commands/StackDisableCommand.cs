using System.CommandLine;

namespace stackmgr.Commands;

public class StackDisableCommand : Command
{
    public StackDisableCommand() : base("disable", "Disable a stack")
    {
        SetAction(v =>
        {
            
        });
    }
}