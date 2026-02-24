using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands;

public class NewCommand : StackManagerCommand
{
    public NewCommand() : base("new", "Create a new resource (environment, stack, app)")
    {
        var env = new StackManagerCommand("environment", "Create a new environment")
        {
            new EnvironmentArgument()
        };
        env.Aliases.Add("env");
        env.SetAction(New);
        
        var stack = new StackManagerCommand("stack", "Create a new stack")
        {
            new EnvironmentOption(),
            new StackArgument()
        };
        stack.Aliases.Add("s");
        stack.SetAction(New);

        var app = new StackManagerCommand("app", "Create a new application")
        {
            new EnvironmentOption(),
            new StackOption(),
            new AppArgument(),
            new TemplateOption(),
            new DevOption(),
            new VolumeOption(),
            new ConfigOption(),
            new HostOption(),
            new PortOption()
        };
        app.SetAction(New);

        var image = new StackManagerCommand("image", "Add a new image")
        {
            new EnvironmentOption(),
            new StackOption(),
            new ImageArgument(),
            new NameOption()
        };
        image.SetAction(New);

        var ingress = new StackManagerCommand("ingress", "Create an ingress for an application")
        {
            new EnvironmentOption(),
            new StackOption(),
            new HostnameArgument(),
            new PortOption(),
            new AppOption(),
            new RedirectOption(),
            new AnnotationOption(),
            new SecuredOption()
        };
        ingress.SetAction(New);
        
        Add(env);
        Add(stack);
        Add(app);
        Add(image);
        Add(ingress);
    }

    private async Task New(ParseResult parseResult)
    {
        if (parseResult.CommandResult.Command.Name == "environment")
        {
            NewEnvironment(parseResult);
            return;
        }
        
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        if (parseResult.CommandResult.Command.Name == "stack")
        {
            NewStack(parseResult, env);
            return;
        }
        
        var stack = GetStack<StackOption>(parseResult, env);
        switch (parseResult.CommandResult.Command.Name)
        {
            case "app":
                await NewApp(parseResult, stack);
                return;
            case "image":
                NewImage(parseResult, stack);
                return;
            case "ingress":
                NewIngress(parseResult, stack);
                return;
        }
    }

    private void NewEnvironment(ParseResult parseResult)
    {
        var name = GetEnvironmentName<EnvironmentArgument>(parseResult);
        var env = StackEnvironment.Create(name);
        HelperMethods.LogSuccess($"Environment '{env.Name}' initialized.");
    }
    
    private void NewStack(ParseResult parseResult, StackEnvironment env)
    {
        var name = GetStackName<StackArgument>(parseResult);
        var stack = Stack.Create(env, name);
        HelperMethods.LogSuccess($"Stack '{stack.Name}' created.");
    }

    private async Task NewApp(ParseResult parseResult, Stack stack)
    {
        var name = GetAppName<AppArgument>(parseResult);
        var template = parseResult.GetValue<string, TemplateOption>();
        var branch = parseResult.GetValue<bool, DevOption>() ? "dev" : "prod";

        if(template is not null)
            template = $"{branch}:{template}";
        
        var options = new StackAppOptions
        {
            Volume = parseResult.GetValue<string, VolumeOption>(),
            Host = parseResult.GetValue<string, HostOption>(),
            Port = parseResult.GetValue<short, PortOption>(),
            Template = template,
            Config = parseResult.GetValue<string[], ConfigOption>() ?? []
        };
        var app = StackApp.Create(stack, name, options);
        HelperMethods.LogSuccess($"App '{app.Name}' created.");
        
        if (template is not null)
        {
            await app.Migrate();
            HelperMethods.LogSuccess("Migration done.");
        }
    }
    
    private void NewImage(ParseResult parseResult, Stack stack)
    {
        var image = StackImage.Create(
            stack, 
            parseResult.GetRequiredValue<string, ImageArgument>(),
            parseResult.GetValue<string, NameOption>()
        );
        HelperMethods.LogSuccess($"Image '{image.Image}' with name '{image.Name}' added.");
    }
    
    private void NewIngress(ParseResult parseResult, Stack stack)
    {
        var hostname = parseResult.GetRequiredValue<string, HostnameArgument>();
        var redirect = parseResult.GetValue<string, RedirectOption>();
        var app = parseResult.GetValue<string, AppOption>();

        if (app is not null && redirect is not null)
        {
            HelperMethods.LogWarning(
                $"Both {HelperMethods.GetSymbolName<AppOption>()} and {HelperMethods.GetSymbolName<RedirectOption>()} specified. {HelperMethods.GetSymbolName<RedirectOption>()} will be ignored.");
        }
        
        if (app is { Length: > 0 })
        {
            var port = parseResult.GetRequiredValue<short, PortOption>();

            StackIngress.Create(stack, hostname, app, port, parseResult.GetValue<bool, SecuredOption>());
        }
        else if (redirect is { Length: > 0 })
        {
            StackIngress.Create(stack, hostname, redirect);
            StackRedirect.Create(stack, redirect);
        }
        else
        {
            throw new Exception($"Either {HelperMethods.GetSymbolName<AppOption>()} or {HelperMethods.GetSymbolName<RedirectOption>()} must be specified.");
        }

        HelperMethods.LogSuccess($"Ingress '{hostname}' created.");
    }
}