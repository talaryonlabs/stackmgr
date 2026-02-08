using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class AppMigrateCommand : StackManagerCommand
{
    public AppMigrateCommand() : base("migrate", "Migrate an application from templates")
    {
        SetAction(v =>
        {
            var env = GetEnvironment<EnvironmentOption>(v);
            var stack = GetStack<StackArgument>(v, env);
        });
    }
}