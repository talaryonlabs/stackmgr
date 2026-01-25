using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class StackListCommand : Command
{
    public StackListCommand() : base("list", "List stacks")
    {
        SetAction(v =>
        {
            var config = StackMgrConfig.Load();
            var name = v.GetRequiredValue<string, EnvironmentArgument>().ToLower();
            var env = config.Environments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            
            if (env is null)
            {
                Console.WriteLine($"Environment '{name}' does not exist.");
                return;
            }
            
            Console.WriteLine($"Listing stacks for {env.Name}");
            var path = Path.Combine(Environment.CurrentDirectory, env.Name.ToLower());
            foreach (var stack in Directory.GetDirectories(path))
            {
                Console.WriteLine(Path.GetFileName(stack));
            }
        });
    }
}