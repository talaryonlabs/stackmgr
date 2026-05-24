using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Validation;

namespace Talaryon.StackManager.Commands.Volumes;

/// <summary>
/// Command for creating a new volume.
/// </summary>
public class NewVolumeCommand : ResourceCreateCommand<StackVolume, VolumeArgument>
{
    public NewVolumeCommand()
        : base("volume", "Create a new volume")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
        Add(new SizeOption());
        Add(new AccessModeOption());
        Add(new ReplicasOption());
    }

    protected override StackVolume CreateResourceInstance(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var name = GetName<VolumeArgument>(parseResult);
        
        ValidationHelper.ValidateAppName(name);
        
        var accessMode = (parseResult.GetValue<string, AccessModeOption>() ?? "ReadWriteOnce").Trim().ToLower() switch
        {
            "rwo" => "ReadWriteOnce",
            "readwrite" => "ReadWriteOnce",
            "readwriteone" => "ReadWriteOnce",
            "readwriteonce" => "ReadWriteOnce",
            "rwx" => "ReadWriteMany",
            "readwritemany" => "ReadWriteMany",
            _ => throw new ArgumentException("Invalid access mode. (ReadWriteOnce, ReadWriteMany)")
        };
        
        var size = parseResult.GetValue<string, SizeOption>() ?? "1Gi";
        size = ValidationHelper.ValidateAndNormalizeSize(size);
        
        var replicas = parseResult.GetValue<int, ReplicasOption>();
        return replicas < 0 ? throw new ArgumentException("Replicas must be >= 0") : StackVolume.Create(stack, name, size, accessMode, replicas);
    }

    protected override void OnResourceCreated(StackVolume resource)
    {
        LogMessage.AsSuccess($"Volume '{resource.Name}' created.");
    }
}
