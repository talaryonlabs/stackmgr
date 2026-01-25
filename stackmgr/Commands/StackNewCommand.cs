using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;

namespace stackmgr.Commands;

public class StackNewCommand : Command
{
    public StackNewCommand() : base("new", "Create a new stack")
    {
        SetAction(v =>
        {
            var env = v.GetRequiredValue<StackEnvironment, EnvironmentOption>();
            var stackName = v.GetRequiredValue<string, NameArgument>();

            if (env.HasStack(stackName))
            {
                Console.WriteLine($"Stack '{stackName}' already exists in environment '{env}'");
                return;
            }
            
            Console.WriteLine($"Creating stack '{stackName}' in environment '{env}'");
            var stackPath = env.GetStackPath(stackName);
            Directory.CreateDirectory(stackPath);
            
            StackConfig.Generate(env, stackName);
        });
    }
}