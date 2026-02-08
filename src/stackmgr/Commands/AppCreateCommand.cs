using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class AppCreateCommand : StackManagerCommand
{
    public AppCreateCommand() : base("create", "Create an application")
    {
        Add(new TemplateOption());
        
        SetAction(v =>
        {
            var env = GetEnvironment<EnvironmentOption>(v);
            var stack = GetStack<StackArgument>(v, env);
            var name = GetAppName<AppArgument>(v);
            
            
            Console.WriteLine(name);

        });
    }
}