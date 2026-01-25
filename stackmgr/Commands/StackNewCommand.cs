using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;

namespace stackmgr.Commands;

public class StackNewCommand : Command
{
    public StackNewCommand() : base("new", "Create a new stack")
    {
        SetAction(async v =>
        {
            var config = StackMgrConfig.Load();
            var name = v.GetRequiredValue<string, EnvironmentOption>().ToLower();
            var env = config.Environments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            var stack = v.GetRequiredValue<string, StackArgument>();
            
            if (env is null)
            {
                Console.WriteLine($"Environment '{name}' does not exist.");
                return;
            }
            
            var path = env.GetStackPath(stack);
            var ns = $"{env.Name.ToLower()}-{stack}";
            
            Console.WriteLine($"Creating stack '{stack}' in environment '{env.Name}'");
            if(!Directory.Exists(path))
            {
                Console.WriteLine($".. Creating directory '{path}'");
                Directory.CreateDirectory(path);
            }
            
            if (await RKE2.CreateNamespace(env, ns))
            {
                Console.WriteLine(".. Stack namespace created.");
            }
            
            // StackConfig.Generate(env, stack);
            
            
        });
    }
}