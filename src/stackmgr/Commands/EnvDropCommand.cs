using System.CommandLine;
using stackmgr.Arguments;

namespace stackmgr.Commands;

public class EnvDropCommand : StackManagerCommand
{
    public EnvDropCommand() : base("drop", "Drop an environment (without environment directory)")
    {
        Add(new EnvironmentArgument());
        SetAction(v =>
        {
            var env = GetEnvironment<EnvironmentArgument>(v);
            HelperMethods.LogWarning($"Are you sure you want to delete environment '{env.Name}'? [y/N] ");
            var input = Console.ReadLine();
            if (input is not null && input.Trim().Length > 0 && input.Trim().Equals("y", StringComparison.CurrentCultureIgnoreCase))
            {
                HelperMethods.LogInfo($"Dropping environment '{env.Name}' ...");
                
                Config.Environments.Remove(env);
                Config.Save();
                HelperMethods.LogSuccess("Success.");
                return;
            }
            HelperMethods.LogInfo("Aborted.");
        });
    }
}