using System.CommandLine;
using stackmgr.Arguments;

namespace stackmgr.Commands;

public class EnvInitCommand : StackManagerCommand
{
    public EnvInitCommand() : base("init", "Initialize a new environment (e.g. dev, prod)")
    {
        Add(new EnvironmentArgument());
        SetAction(v =>
        {
            var env = v.GetRequiredValue<string, EnvironmentArgument>().ToLower();
            var path = Path.Combine(Environment.CurrentDirectory, env);
            var directory = new DirectoryInfo(path);
            
            if (!directory.Exists)
                directory.Create();
            
            if (Config.Environments.Any(x => x.Name.Equals(env, StringComparison.CurrentCultureIgnoreCase)))
            {
                Console.WriteLine($"Environment '{env}' already exists.");
                return;
            }
            
            Console.WriteLine($"Initializing environment '{env}' ...");
            Config.Environments.Add(new StackEnvironment { Name = env });
            Config.Save();
            Console.WriteLine("Success.");
        });
    }
}
