using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Services;
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
        var branch = parseResult.GetValue<bool, DevOption>() ? "dev" : "prod";
        
        StackApp app;
        
        if (template is not null)
        {
            var stackTemplate = StackTemplate.Load(template);
            var appTemplate = new StackAppTemplate
            {
                Name = stackTemplate.Name,
                Branch = branch,
            };
            
            app = StackApp.Create(stack, name, appTemplate);
            
            // Initialize the app with template contents in .base folder
            var appService = new AppService(app);
            appService.InitializeFromTemplateAsync(stackTemplate).GetAwaiter().GetResult();
            
            LogMessage.AsSuccess($"App '{app.Name}' created with template '{stackTemplate.Name}'.");
            LogMessage.AsInfo($"Template files are in the '.base' folder.");
        }
        else
        {
            app = StackApp.Create(stack, name, null);
            LogMessage.AsSuccess($"App '{app.Name}' created.");
        }
        
        return app;
    }

    protected override void OnResourceCreated(StackApp resource)
    {
        if (resource.Template is not null)
        {
            LogMessage.AsInfo($"To update the app from template, run: stackmgr migrate app {resource.Name}");
        }
    }
}
