using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class StackDeleteCommand : Command
{
    public StackDeleteCommand() : base("delete", "Delete a stack")
    {
        SetAction(v =>
        {
            var env = v.GetRequiredValue<StackEnvironment, EnvironmentOption>();
            var stackName = v.GetRequiredValue<string, StackNameArgument>();

            if (!env.HasStack(stackName))
            {
                Console.WriteLine($"Stack '{stackName}' does not exist in environment '{env}'");
                return;
            }
            Console.Write($"Are you sure you want to delete stack '{stackName}' in environment '{env}'? [y/N] ");
            var input = Console.ReadLine();
            if (input is not null && input.Trim().Length > 0 && input.Trim().Equals("y", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine($"Deleting stack '{stackName}' in environment '{env}'");
                var stackPath = env.GetStackPath(stackName);
                Directory.Delete(stackPath, true);
                return;
            }
            Console.WriteLine("Aborted");
        });
    }
}