using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Apps;

/// <summary>
/// Command for configuring an app.
/// </summary>
public class ConfigureAppCommand : ResourceConfigureCommand<AppArgument>
{
    public ConfigureAppCommand()
        : base("app", "Configure an app")
    {
        Add(new EnvironmentOption { Required = true });
        Add(new StackOption { Required = true });
        Add(new ParamOption());
        Add(new RequirementOption());
        Add(new VolumeOption());
        Add(new ImageOption());
    }

    protected override void Configure(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var app = GetApp<AppArgument>(parseResult, stack);

        var volumes = VolumeOption.GetVolumes(parseResult);
        foreach (var volume in volumes)
        {
            app.Volumes[volume.Key] = volume.Value;
            LogMessage.AsSuccess($"Volume '{volume.Key}' set to '{volume.Value}' for app '{app.Name}'.");
        }
        
        var requirements = RequirementOption.GetRequirements(parseResult);
        foreach (var requirement in requirements)
        {
            app.Requirements[requirement.Key] = requirement.Value;
            LogMessage.AsSuccess($"Requirement '{requirement.Key}' set to '{requirement.Value}' for app '{app.Name}'.");
        }
        
        var parameters = ParamOption.GetParams(parseResult);
        foreach (var parameter in parameters)
        {
            app.Params[parameter.Key] = parameter.Value;
            LogMessage.AsSuccess($"Parameter '{parameter.Key}' set to '{parameter.Value}' for app '{app.Name}'.");
        }

        var images = ImageOption.GetImages(parseResult);
        foreach (var image in images)
        {
            app.Images[image.Key] = image.Value;
            LogMessage.AsSuccess($"Image '{image.Key}' set to '{image.Value}' for app '{app.Name}'.");
        }
        
        stack.SaveConfig();
    }
}
