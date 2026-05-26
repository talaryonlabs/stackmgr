using System.CommandLine;
using System.IO.Pipes;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Images;

/// <summary>
/// Command for migrating an image to a new version.
/// </summary>
public class MigrateImageCommand : ResourceMigrateCommand<StackImage, ImageArgument>
{
    public MigrateImageCommand()
        : base("image", "Migrate an image to a new version")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
        Add(new NameOption());
    }

    protected override StackImage LoadResource()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        var newImage = GetValue<string, ImageArgument>();
        var name = GetValue<string, NameOption>();
        
        if (string.IsNullOrEmpty(name))
        {
            var parts = newImage.Split("/");
            name = parts[^1].Contains(':') ? parts[^1].Split(":")[0] : parts[^1];
        }

        return stack.Images.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ??
               throw new Exception($"Image '{name}' not found in stack '{stack.Name}' (environment '{env.Name}').");
    }

    protected override void MigrateResource(StackImage resource)
    {
        var newImage = GetRequiredValue<string, ImageArgument>();
        var name = resource.Name;

        resource.Image = newImage;
        resource.Stack.Save();
        
        LogMessage.AsSuccess($"Image '{name}' migrated to '{newImage}'.");
    }
}
