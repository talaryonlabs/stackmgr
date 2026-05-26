using Talaryon.StackManager.Builder;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Validation;

namespace Talaryon.StackManager.Commands.Environments;

/// <summary>
/// Command for creating a new environment.
/// </summary>
public class NewEnvironmentCommand : ResourceCreateCommand<StackEnvironment, EnvironmentArgument>
{
    public NewEnvironmentCommand() 
        : base("environment", "Create a new environment")
    {
        Aliases.Add("env");
    }

    protected override StackEnvironment CreateResourceInstance()
    {
        var name = GetName<EnvironmentArgument>();
        ValidationHelper.ValidateEnvironmentName(name);

        var env = new StackEnvironmentBuilder()
            .WithName(name)
            .Build();

        env.Save();
        
        return env;
    }

    protected override void OnResourceCreated(StackEnvironment resource)
    {
        LogMessage.AsSuccess($"Environment '{resource.Name}' initialized.");

        var config = GetRequiredService<LocalConfig>();
        config.Defaults.Environment = resource.Name;
        config.Save();
        LogMessage.AsInfo($"Default environment set to '{resource.Name}'.");
    }
}
