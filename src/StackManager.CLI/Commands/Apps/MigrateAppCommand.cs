using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Commands.Apps;

/// <summary>
/// Command for migrating an app from its template.
/// </summary>
public class MigrateAppCommand : ResourceMigrateCommand<StackApp, AppArgument>
{
    public MigrateAppCommand()
        : base("app", "Migrate an app from a template")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
    }

    protected override StackApp LoadResource()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        return GetApp<AppArgument>(stack);
    }

    protected override async Task MigrateResourceAsync(StackApp resource)
    {
        var app = resource;
        var stack = app.Stack;
        var env = stack.Environment;

        if (app.Template is null)
        {
            LogMessage.AsWarning($"App '{app.Name}' has no template.");
            return;
        }

        var templateService = GetRequiredService<ITemplateService>();
        await templateService.UpdateAsync(app.Template.Name);
        
        var template = templateService.GetTemplate(app.Template.Name);
        var errors = new List<string>();

        errors.AddRange(template.Volumes
            .Where(volume => !app.Volumes.ContainsKey(volume))
            .Select(volume =>
                $"Missing volume '{volume}'. Run: stackmgr configure app {app.Name} --volume {volume}:<name>")
        );

        errors.AddRange(template.Requirements
            .Where(requirement => !app.Requirements.ContainsKey(requirement))
            .Select(requirement =>
                $"Missing requirement '{requirement}'. Run: stackmgr configure app {app.Name} --requirement {requirement}:<name>")
        );

        errors.AddRange(template.Params
            .Where(parameter => !app.Params.ContainsKey(parameter))
            .Select(parameter =>
                $"Missing parameter '{parameter}'. Run: stackmgr configure app {app.Name} --param {parameter}:<value>")
        );

        errors.AddRange(template.Images
            .Where(image => !stack.Images.Exists(v => v.Name == image))
            .Select(image =>
                $"Missing image '{image}' in stack '{stack.Name}' (environment '{env.Name}').")
        );

        if (errors.Count > 0)
        {
            errors.ForEach(LogMessage.AsError);
            return;
        }

        if (!await app.CheckRequirements(template))
        {
            return;
        }

        await LogBuilder
            .Message($"Migrating app '{app.Name}' from template '{app.Template.Name}' ({app.Template.Branch}) ... ")
            .NoNewLineAfter()
            .WaitFor(() =>
            {
                templateService.ApplyTemplate(template, app);
                return LogBuilder.Message("Migration done.").AsSuccess();
            })
            .RunAsync();
    }
}
