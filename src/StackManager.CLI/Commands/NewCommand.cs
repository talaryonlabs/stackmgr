using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Services;
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
            new StackArgument(),
            new AppArgument(),
            new TemplateOption(),
            new DevOption(),
            new VolumeOption(),
            new ConfigOption(),
            new HostOption(),
            new PortOption(),
            new WithoutIngressOption()
        };
        app.SetAction(New);

        var image = new StackManagerCommand("image", "Add a new image")
        {
            new EnvironmentOption(),
            new StackArgument(),
            new ImageArgument(),
            new NameOption()
        };
        image.SetAction(New);

        var ingress = new StackManagerCommand("ingress", "Create an ingress for an application")
        {
            new EnvironmentOption(),
            new StackArgument(),
            new HostArgument(),
            new PortOption(),
            new NameOption(),
            new RedirectToOption(),
            new AnnotationOption(),
            new SecuredOption()
        };
        
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
        
        var stack = GetStack<StackArgument>(parseResult, env);
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
        var env = StackEnvironment.New(name);

        if (!env.LocalDirectory.Exists)
        {
            env.LocalDirectory.Create();
            HelperMethods.LogSuccess($"Directory '{env.LocalDirectory.FullName}' created.");
        }
            
        if (File.Exists(Path.Combine(env.LocalDirectory.FullName, StackEnvironment.FileName)))
        {
            HelperMethods.LogWarning($"Environment '{env.Name}' already exists.");
            return;
        }
            
        HelperMethods.LogInfo($"Initializing environment '{env.Name}' ...");
        env.SaveConfig();
        HelperMethods.LogSuccess("Success.");
    }
    
    private void NewStack(ParseResult parseResult, StackEnvironment env)
    {
        var name = GetStackName<StackArgument>(parseResult);
        var stack = Stack.New(env, name);

        if (stack.LocalDirectory.Exists)
        {
            throw new StackAlreadyExistsException(stack);
        }

        HelperMethods.LogInfo($"Creating stack '{stack.Name}' in environment '{env.Name}'.");
        stack.LocalDirectory.Create();
        
        if (!stack.LocalDirectory.Exists)
        {
            HelperMethods.LogError("Failed.");
            return;
        }
        stack.SaveConfig();
        stack.SaveKustomization();
            
        HelperMethods.LogSuccess("Done.");
    }

    private async Task NewApp(ParseResult parseResult, Stack stack)
    {
        var name = GetAppName<AppArgument>(parseResult);
        try
        {
            var a = GetApp<AppArgument>(parseResult, stack);
            throw new AppAlreadyExistsException(stack, a);
        }
        catch (AppNotFoundException) { }

        var path = Path.Combine(stack.LocalDirectory.FullName, name);
        var dir = new DirectoryInfo(path);
            
        if(!dir.Exists) dir.Create();

        var template = parseResult.GetValue<string, TemplateOption>();
        var branch = parseResult.GetValue<bool, DevOption>() ? "dev" : "prod";
        var app = new StackApp()
        {
            Name = name,
            Volume = parseResult.GetValue<string, VolumeOption>() ?? "",
            Host = parseResult.GetValue<string, HostOption>() ?? "",
            Port = parseResult.GetValue<short, PortOption>(),
            Template = template is not null ? $"{branch}:{template}" : "",
            Config = (parseResult.GetValue<string[], ConfigOption>() ?? []).Select(x =>
            {
                var config = x.Split("=");
                return new StackAppConfig
                {
                    Name = config[0].Trim(),
                    Value = config.Length > 1 ? config[1].Trim() : ""
                };
            }).ToList()
        };
        
        if (template is not null)
        {
            var options = new AppServiceOptions
            {
                WithoutIngress = parseResult.GetValue<bool, WithoutIngressOption>()
            };
            var appService = new AppService(stack, app);
            await appService.Install(options);
            
            HelperMethods.LogSuccess("Installation done.");
        }
        
        stack.Apps.Add(app);
        stack.SaveConfig();
        stack.SaveKustomization();
        
        HelperMethods.LogSuccess("App created.");
    }
    
    private void NewImage(ParseResult parseResult, Stack stack)
    {
        var name = parseResult.GetValue<string, NameOption>();
        var image = parseResult.GetRequiredValue<string, ImageArgument>();

        if (string.IsNullOrEmpty(name))
        {
            var parts = image.Split("/");
            name = parts[^1].Contains(':') ? parts[^1].Split(":")[0] : parts[^1];
        }

        if (stack.Images.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new Exception($"Image with name '{name}' already exists in stack '{stack.Name}' (use 'stackmgr migrate image' instead)");
        }
        
        stack.Images.Add(new()
        {
            Name = name,
            Image = image
        });
        stack.SaveConfig();
        stack.SaveKustomization();
        
        HelperMethods.LogSuccess($"Image '{image}' with name '{name}' added.");
    }
    
    private void NewIngress(ParseResult parseResult, Stack stack)
    {
        var host = parseResult.GetRequiredValue<string, HostArgument>();
        if (stack.Ingresses.Any(x => x.Host.Equals(host, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new Exception($"Ingress with host '{host}' already exists in stack '{stack.Name}'.");
        }

        var redirect = parseResult.GetValue<string, RedirectToOption>();
        var name = parseResult.GetValue<string, NameOption>();
        
        if (name is { Length: > 0 })
        {
            var port = parseResult.GetRequiredValue<string, PortOption>();
            
            stack.Ingresses.Add(new StackIngress
            {
                Host = host,
                Service = name,
                Port = port,
                IsSecured = parseResult.GetValue<bool, SecuredOption>(),
                Annotations = (parseResult.GetValue<string[], AnnotationOption>() ?? []).Select(x =>
                {
                    var annotation = x.Split("=");
                    return new KeyValuePair<string, string>(annotation[0].Trim(), annotation[1].Trim());
                }).ToDictionary()
            });
        }
        else if (redirect is { Length: > 0 })
        {
            stack.Ingresses.Add(new StackIngress
            {
                Host = host,
                RedirectTo = redirect
            });
        }
        else
        {
            throw new Exception("Either --name or --redirect-to must be specified.");
        }
        
        
        
        
        
        
        stack.SaveConfig();
        stack.SaveKustomization();
        
        HelperMethods.LogSuccess($"Ingress '{host}' created.");
    }
}