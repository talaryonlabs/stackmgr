using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class StackNewCommand : StackManagerCommand
{
    public StackNewCommand() : base("new", "Create a new stack")
    {
        SetAction(v =>
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
            if(Directory.Exists(path))
            {
                Console.WriteLine($"Stack '{stack}' already exists in environment '{env.Name}'");
                return;
            }
            
            Console.Write($"Creating stack '{stack}' in environment '{env.Name}' .. ");
            if (Directory.CreateDirectory(path) is { Exists: false })
            {
                Console.WriteLine("Failed.");
                return;
            }
            Console.WriteLine("Done.");
        });
    }
}