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
            new DevOption()
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
            new SecuredOption(),
            new GenerateOption()
        };
        ingress.SetAction(New);

        var volume = new StackManagerCommand("volume", "Create a new volume")
        {
            new EnvironmentOption(),
            new StackOption(),
            new VolumeArgument(),
            new SizeOption(),
            new AccessModeOption(),
            new ReplicasOption()
        };
        volume.SetAction(New);       
        
        Add(env);
        Add(stack);
        Add(app);
        Add(image);
        Add(ingress);
        Add(volume);       
    }

    private void New(ParseResult parseResult)
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
                NewApp(parseResult, stack);
                return;
            case "image":
                NewImage(parseResult, stack);
                return;
            case "volume":
                NewVolume(parseResult, stack);
                return;
            case "ingress":
                NewIngress(parseResult, stack);
                return;
        }
    }

    private void NewEnvironment(ParseResult parseResult)
    {
        var name = GetName<EnvironmentArgument>(parseResult);
        var env = StackEnvironment.Create(name);
        LogMessage.AsSuccess($"Environment '{env.Name}' initialized.");
    }
    
    private void NewStack(ParseResult parseResult, StackEnvironment env)
    {
        var name = GetName<StackArgument>(parseResult);
        var stack = Stack.Create(env, name);
        LogMessage.AsSuccess($"Stack '{stack.Name}' created.");
    }

    private void NewApp(ParseResult parseResult, Stack stack)
    {
        var name = GetName<AppArgument>(parseResult);
        var template = parseResult.GetValue<string, TemplateOption>();
        var appTemplate = template is null ? null : new StackAppTemplate
        {
            Name = StackTemplate.Load(template).Name,
            Branch = parseResult.GetValue<bool, DevOption>() ? "dev" : "prod",
        };

        var app = StackApp.Create(stack, name, appTemplate);
        LogMessage.AsSuccess($"App '{app.Name}' created.");

        if (appTemplate is not null)
        {
            LogMessage.AsWarning($"Call 'stackmgr migrate app {app.Name}'.");
        }
    }
    
    private void NewImage(ParseResult parseResult, Stack stack)
    {
        var image = StackImage.Create(
            stack, 
            GetName<ImageArgument>(parseResult),
            parseResult.GetValue<string, NameOption>()
        );
        LogMessage.AsSuccess($"Image '{image.Image}' with name '{image.Name}' added.");
    }
    
    private void NewVolume(ParseResult parseResult, Stack stack)
    {
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
        if (long.TryParse(size, out var parsedSize))
        {
            size = $"{parsedSize}Gi";       
        }
        
        var replicas = parseResult.GetValue<int, ReplicasOption>();
        var volume = StackVolume.Create(
            stack, 
            GetName<VolumeArgument>(parseResult),
            size,
            accessMode,
            replicas
        );
        LogMessage.AsSuccess($"Volume '{volume.Name}' created.");       
    }
    
    private void NewIngress(ParseResult parseResult, Stack stack)
    {
        var hostname = GetName<HostnameArgument>(parseResult);
        var redirect = parseResult.GetValue<string, RedirectOption>();
        var app = parseResult.GetValue<string, AppOption>();
        
        if(parseResult.GetValue<bool, GenerateOption>())
        {
            if(hostname.StartsWith("."))
                hostname = hostname[1..];

            for (var i = 0; i < 10; i++)
            {
                var generated = $"{HelperMethods.GenerateRandomHostname()}-{stack.Name.ToLower()}.{hostname}";
                if (stack.Ingresses.Count(v =>
                        v.Hostname.Equals(generated, StringComparison.InvariantCultureIgnoreCase)) == 0)
                {
                    hostname = generated;
                    break;
                }
            }
        }

        if (app is not null && redirect is not null)
        {
            LogMessage.AsWarning(
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

        LogMessage.AsSuccess($"Ingress '{hostname}' created.");
    }
}