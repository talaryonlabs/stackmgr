using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;

namespace stackmgr.Commands;

public class StackDeleteCommand : StackManagerCommand
{
    public StackDeleteCommand() : base("delete", "Delete a stack")
    {
        SetAction(async v =>
        {
            var env = GetEnvironment<EnvironmentOption>(v);
            var stack = GetStack<StackArgument>(v, env);

            HelperMethods.LogWarning($"Are you sure you want to delete stack '{stack}' in environment '{env.Name}'? [y/N] ");
            var input = Console.ReadLine();
            if (!(input is not null && input.Trim().Length > 0 && input.Trim().Equals("y", StringComparison.CurrentCultureIgnoreCase)))
            {
                HelperMethods.LogInfo("Aborted");
                return;
            }
            HelperMethods.LogWarning($"Deleting stack '{stack.Name}' in environment '{env.Name}':");

            if (await RKE2.DeleteNamespace(env, stack.Namespace))
            {
                
                
            }
            stack.LocalDirectory.Delete(true);
            HelperMethods.LogSuccess("Done.");
        });
    }
}