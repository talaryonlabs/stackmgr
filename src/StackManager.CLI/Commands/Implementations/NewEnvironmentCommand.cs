using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Base;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;
using Talaryon.StackManager.Validation;

namespace Talaryon.StackManager.Commands.Implementations;

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

    protected override StackEnvironment CreateResourceInstance(ParseResult parseResult)
    {
        var name = GetName<EnvironmentArgument>(parseResult);
        ValidationHelper.ValidateEnvironmentName(name);
        return StackEnvironment.Create(name);
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
