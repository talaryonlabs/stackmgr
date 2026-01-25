using System.CommandLine;
using stackmgr.Arguments;

namespace stackmgr.Commands;

public class EnvInitCommand : Command
{
    public EnvInitCommand() : base("init", "Initialize a new environment (e.g. dev, prod)")
    {
        Add(new EnvironmentArgument());
        SetAction(v =>
        {
            var config = StackMgrConfig.Load();
            var env = v.GetRequiredValue<string, EnvironmentArgument>().ToLower();
            var path = Path.Combine(Environment.CurrentDirectory, env);
            var directory = new DirectoryInfo(path);
            
            if (!directory.Exists)
                directory.Create();
            
            if (config.Environments.Any(x => x.Name.Equals(env, StringComparison.CurrentCultureIgnoreCase)))
            {
                Console.WriteLine($"Environment '{env}' already exists. ");
                return;
            }
            
            Console.WriteLine($"Initializing environment '{env}' ...");
            config.Environments.Add(new StackEnvironment { Name = env });
            config.Save();
            Console.WriteLine("Success.");
        });
    }
}
