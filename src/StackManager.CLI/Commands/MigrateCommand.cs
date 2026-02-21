using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Commands;

public class MigrateCommand : StackManagerCommand
{
    public MigrateCommand() : base("migrate", "Migrate an resource (app)")
    {
        var app = new StackManagerCommand("app", "Migrate an app from a template")
        {
            new EnvironmentOption(),
            new StackArgument(),
            new AppArgument(),
            new WithoutIngressOption()
        };
        app.SetAction(MigrateApp);
        
        var image = new StackManagerCommand("image", "Migrate an image to a new version")
        {
            new EnvironmentOption(),
            new StackArgument(),
            new ImageArgument(),
            new NameOption()
        };
        image.SetAction(MigrateImage);
        
        Add(app);
        Add(image);
    }

    private void MigrateImage(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackArgument>(parseResult, env);
        var image = parseResult.GetRequiredValue<string, ImageArgument>();
        var name = parseResult.GetValue<string, NameOption>();
        
        if (string.IsNullOrEmpty(name))
        {
            var parts = image.Split("/");
            name = parts[^1].Contains(':') ? parts[^1].Split(":")[0] : parts[^1];
        }
        
        var local = stack.Images.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        if (local is null)
        {
            HelperMethods.LogWarning($"Image '{name}' not found in stack '{stack.Name}' (environment '{env.Name}').");
            return;
        }
        
        local.Image = image;
        stack.SaveConfig();
        stack.SaveKustomization();
        HelperMethods.LogSuccess($"Image '{name}' migrated to '{image}'.");
    }

    private async Task MigrateApp(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackArgument>(parseResult, env);
        var app = GetApp<AppArgument>(parseResult, stack);
        
        
        HelperMethods.LogInfo($"Migrating app '{app.Name}' from template '{app.Template}' ({stack.Name} in environment '{env.Name}')");

        var appService = new AppService(stack, app);
        await appService.Migrate(new AppServiceOptions()
        {
            WithoutIngress = parseResult.GetValue<bool, WithoutIngressOption>()
        });
        
        stack.SaveKustomization();
        
        HelperMethods.LogSuccess("Migration done.");
    }
    
}