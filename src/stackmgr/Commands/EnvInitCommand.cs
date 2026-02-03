using stackmgr.Arguments;

namespace stackmgr.Commands;

public class EnvInitCommand : StackManagerCommand
{
    public EnvInitCommand() : base("init", "Initialize a new environment (e.g. dev, prod)")
    {
        Add(new EnvironmentArgument());
        SetAction(v =>
        {
            var name = GetEnvironmentName<EnvironmentArgument>(v);
            var env = new StackEnvironment { Name = name };

            if (!env.LocalDirectory.Exists)
            {
                env.LocalDirectory.Create();
                HelperMethods.LogSuccess($"Directory '{env.LocalDirectory.FullName}' created.");
            }
            
            if (Config.Environments.Any(x => x.Name.Equals(env.Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                HelperMethods.LogWarning($"Environment '{env.Name}' already exists.");
                return;
            }
            
            HelperMethods.LogInfo($"Initializing environment '{env.Name}' ...");
            Config.Environments.Add(env);
            Config.Save();
            HelperMethods.LogSuccess("Success.");
        });
    }
}
