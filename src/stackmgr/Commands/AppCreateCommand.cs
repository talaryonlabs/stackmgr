using stackmgr.Options;

namespace stackmgr.Commands;

public class AppCreateCommand : StackManagerCommand
{
    public AppCreateCommand() : base("create", "Create an application")
    {
        Add(new TemplateOption());
    }
}