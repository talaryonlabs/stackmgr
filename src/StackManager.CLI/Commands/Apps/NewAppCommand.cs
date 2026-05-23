using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;
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

    protected override StackApp CreateResourceInstance(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var name = GetName<AppArgument>(parseResult);
        
        ValidationHelper.ValidateAppName(name);
        
        var template = parseResult.GetValue<string, TemplateOption>();
        var appTemplate = template is null ? null : new StackAppTemplate
        {
            Name = StackTemplate.Load(template).Name,
            Branch = parseResult.GetValue<bool, DevOption>() ? "dev" : "prod",
        };

        return StackApp.Create(stack, name, appTemplate);
    }

    protected override void OnResourceCreated(StackApp resource)
    {
        LogMessage.AsSuccess($"App '{resource.Name}' created.");

        if (resource.Template is not null)
        {
            LogMessage.AsWarning($"Call 'stackmgr migrate app {resource.Name}'.");
        }
    }
}
