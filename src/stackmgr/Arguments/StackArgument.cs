using System.CommandLine;

namespace stackmgr.Arguments;

public class StackArgument : Argument<string>
{
    public StackArgument() : base("stack")
    {
        Description = "stack name (e.g. project, customer, company)";
    }
}