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
            var name = v.GetRequiredValue<string, EnvironmentOption>().ToLower();
            var env = Config.Environments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            var stack = v.GetRequiredValue<string, StackArgument>();

            if (env is null)
            {
                Console.WriteLine($"Environment '{name}' does not exist.");
                return;
            }
            
            var path = env.GetStackPath(stack);
            var ns = $"{env.Name.ToLower()}-{stack}";
            
            if (!env.HasLocalStack(stack))
            {
                Console.WriteLine($"Stack '{stack}' does not exist in environment '{env.Name}'");
                return;
            }
            Console.Write($"Are you sure you want to delete stack '{stack}' in environment '{env.Name}'? [y/N] ");
            var input = Console.ReadLine();
            if (!(input is not null && input.Trim().Length > 0 && input.Trim().Equals("y", StringComparison.CurrentCultureIgnoreCase)))
            {
                Console.WriteLine("Aborted");
                return;
            }
            Console.WriteLine($"Deleting stack '{stack}' in environment '{env.Name}'");
            var stackPath = env.GetStackPath(stack);
            // Directory.Delete(stackPath, true);
            
            if (await RKE2.DeleteNamespace(env, ns))
            {
                Console.WriteLine(".. Stack namespace deleted.");
            }
        });
    }
}