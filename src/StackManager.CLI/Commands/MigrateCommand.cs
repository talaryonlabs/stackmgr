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
            new StackOption(),
            new AppArgument()
        };
        app.SetAction(MigrateApp);
        
        var image = new StackManagerCommand("image", "Migrate an image to a new version")
        {
            new EnvironmentOption(),
            new StackOption(),
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
        var stack = GetStack<StackOption>(parseResult, env);
        var newImage = parseResult.GetRequiredValue<string, ImageArgument>();
        var name = parseResult.GetValue<string, NameOption>();
        
        if (string.IsNullOrEmpty(name))
        {
            var parts = newImage.Split("/");
            name = parts[^1].Contains(':') ? parts[^1].Split(":")[0] : parts[^1];
        }
        
        var image = stack.Images.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        if (image is null)
        {
            LogMessage.AsWarning($"Image '{name}' not found in stack '{stack.Name}' (environment '{env.Name}').");
            return;
        }
        
        image.Migrate(newImage);
        LogMessage.AsSuccess($"Image '{name}' migrated to '{newImage}'.");
    }

    private async Task MigrateApp(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var app = GetApp<AppArgument>(parseResult, stack);

        await LogBuilder
            .Message($"Migrating app '{app.Name}' from template '{app.Template}' ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                await app.Migrate();
                return LogBuilder.Message("Migration done.").AsSuccess();
            })
            .RunAsync();
    }
    
}