using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Services;
using Talaryon.StackManager.Types;

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

        if (app.Template is null)
        {
            LogMessage.AsWarning($"App '{app.Name}' has no template.");
            return;
        }

        var git = new GitService();
        await git.GetAppsAsync(app.Template.Branch);
        
        var template = StackTemplate.Load(app.Template.Name);

        var errors = new List<string>();
        foreach (var volume in template.Volumes.Where(volume => !app.Volumes.ContainsKey(volume)))
        {
            errors.Add($"Missing volume '{volume}'. Run: stackmgr configure app {app.Name} --volume {volume}:<name>");
        }

        foreach (var requirement in template.Requirements.Where(requirement => !app.Requirements.ContainsKey(requirement)))
        {
            errors.Add($"Missing requirement '{requirement}'. Run: stackmgr configure app {app.Name} --requirement {requirement}:<name>");
        }
        
        foreach (var parameter in template.Params.Where(parameter => !app.Params.ContainsKey(parameter)))
        {
            errors.Add($"Missing parameter '{parameter}'. Run: stackmgr configure app {app.Name} --param {parameter}:<value>");
        }

        errors.AddRange(template.Images
            .Where(image => !stack.Images.Exists(v => v.Name == image))
            .Select(image =>
                $"Missing image '{image}' in stack '{stack.Name}' (environment '{env.Name}').")
        );

        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                LogMessage.AsError(error);
            }
            return;
        }

        if (!await app.CheckRequirements(template))
        {
            return;
        }

        await LogBuilder
            .Message($"Migrating app '{app.Name}' from template '{app.Template.Name}' ({app.Template.Branch}) ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                await app.Migrate(template);
                return LogBuilder.Message("Migration done.").AsSuccess();
            })
            .RunAsync();
    }
    
}