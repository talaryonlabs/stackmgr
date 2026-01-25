using System.CommandLine;
using stackmgr.Arguments;

namespace stackmgr.Commands;

public class EnvDropCommand : Command
{
    public EnvDropCommand() : base("drop", "Drop an environment (without environment directory)")
    {
        Add(new EnvironmentArgument());
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
            Console.WriteLine($"Dropping environment '{env.Name}' ...");
            
            Console.Write($"Are you sure you want to delete environment '{env.Name}'? [y/N] ");
            var input = Console.ReadLine();
            if (input is not null && input.Trim().Length > 0 && input.Trim().Equals("y", StringComparison.CurrentCultureIgnoreCase))
            {
                config.Environments.Remove(env);
                config.Save();
                Console.WriteLine("Success.");
                return;
            }
            Console.WriteLine("Aborted.");
        });
    }
}