using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Validation;

namespace Talaryon.StackManager.Commands.Apps;

/// <summary>
/// Command for creating a new app.
/// </summary>
public class NewAppCommand : ResourceCreateCommand<StackApp, AppArgument>
{
    public NewAppCommand()
        : base("app", "Create a new application")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
        Add(new TemplateOption());
        Add(new DevOption());
    }

    protected override StackApp CreateResourceInstance()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        var name = GetName<AppArgument>();
        
        ValidationHelper.ValidateAppName(name);
        
        var template = GetValue<string, TemplateOption>();
        var branch = GetValue<bool, DevOption>() ? "dev" : "prod";

        return stack
            .New<StackApp>()
            .WithName(name)
            .Configure(app =>
            {
                if (template is not null)
                {
                    app.Template = new StackAppTemplate
                    {
                        Name = template,
                        Branch = branch,
                    };
                }
            }).Save();
    }

    protected override void OnResourceCreated(StackApp resource)
    {
        LogMessage.AsSuccess($"App '{resource.Name}' created.");
        
        if (resource.Template is not null)
        {
            LogMessage.AsInfo($"To update the app from template, run: stackmgr migrate app {resource.Name}");
        }
    }
}
