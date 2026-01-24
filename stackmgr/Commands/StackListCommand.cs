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
            var env = v.CommandResult.GetRequiredValue<StackEnvironment>(new EnvironmentOption().Name);
            Console.WriteLine($"Listing stacks for {env}");
            
            var path = Path.Combine(Directory.GetCurrentDirectory(), env.ToString().ToLower());
            foreach (var stack in Directory.GetDirectories(path))
            {
                Console.WriteLine(Path.GetFileName(stack));
            }
        });
    }
}