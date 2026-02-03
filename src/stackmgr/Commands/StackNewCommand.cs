using stackmgr.Arguments;
using stackmgr.Exceptions;
using stackmgr.Options;

namespace stackmgr.Commands;

public class StackNewCommand : StackManagerCommand
{
    public StackNewCommand() : base("new", "Create a new stack")
    {
        SetAction(v =>
        {
            var env = GetEnvironment<EnvironmentOption>(v);
            var name = GetStackName<StackArgument>(v);
            var stack = Stack.New(env, name);

            if (stack.LocalDirectory.Exists)
            {
                throw new StackAlreadyExistsException(stack);
            }

            HelperMethods.LogInfo($"Creating stack '{stack.Name}' in environment '{env.Name}'.");
            if (stack.LocalDirectory.Exists)
            {
                HelperMethods.LogError("Failed.");
                return;
            }
            stack.LocalDirectory.Create();
            stack.SaveConfig();
            stack.SaveKustomization();
            
            HelperMethods.LogSuccess("Done.");
        });
    }
}