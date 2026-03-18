using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Services;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands;

public class DescribeCommand : StackManagerCommand
{
    public DescribeCommand() : base("describe", "Describe a resource (environment, stack, template)")
    {
        var env = new StackManagerCommand("environment", "Describe a environment")
        {
            new EnvironmentArgument()
        };
        env.Aliases.Add("env");
        env.SetAction(DescribeEnvironment);

        var stack = new StackManagerCommand("stack", "Describe a stack")
        {
            new EnvironmentOption(),
            new StackArgument()
        };
        stack.SetAction(DescribeStack);
        
        var app = new StackManagerCommand("app", "Describe an application")
        {
            new EnvironmentOption(),
            new StackOption(),
            new AppArgument()
        };
        app.SetAction(DescribeApp);

        var template = new StackManagerCommand("template", "Describe an application template")
        {
            new NameArgument(),
            new DevOption()
        };
        template.SetAction(DescribeTemplate);
        
        Add(env);
        Add(stack);
        Add(app);
        Add(template);
    }

    private void DescribeEnvironment(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentArgument>(parseResult);

        LogMessage.Separator();

        // Use dynamic coloring for environment name
        LogBuilder.Message("Environment: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{env.Name}").AsColored(ConsoleColor.Cyan))
            .Run();

        // Use standard colors for other properties
        LogBuilder.Message(" Vault: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{env.Vault}").AsWarning())
            .Run();

        LogBuilder.Message(" Outpost: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{env.Outpost}").AsWarning())
            .Run();

        LogBuilder.Message(" Cert Issuer: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{env.CertIssuer}").AsWarning())
            .Run();

        LogBuilder.Message(" Registry Credentials: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{env.RegistryCredentials}").AsWarning())
            .Run();

        LogBuilder.Message(" Repository: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{env.Repository ?? "None"}").AsSuccess())
            .Run();

        LogBuilder.Message(" Remote: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{env.Remote}").AsColored(ConsoleColor.Blue))
            .Run();

        LogMessage.Separator();
    }

    private void DescribeApp(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var app = GetApp<AppArgument>(parseResult, stack);

        LogMessage.Separator();

        LogBuilder.Message("App: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{app.Name}").AsSuccess())
            .Run();

        if (app.Template != null)
        {
            LogBuilder.Message(" Template: ")
                .NoNewLineAfter()
                .WaitFor(() => LogBuilder.Message($"{app.Template.Name} ({app.Template.Branch})").AsWarning())
                .Run();
        }

        if (app.Volumes.Count > 0)
        {
            LogBuilder.Message(" Volumes: ")
                .NoNewLineAfter()
                .WaitFor(() => LogBuilder.Message($"[{string.Join(", ", app.Volumes.Select(v => $"{v.Key}:{v.Value}"))}]").AsSuccess())
                .Run();
        }

        if (app.Requirements.Count > 0)
        {
            LogBuilder.Message(" Requirements: ")
                .NoNewLineAfter()
                .WaitFor(() => LogBuilder.Message($"[{string.Join(", ", app.Requirements.Select(r => $"{r.Key}:{r.Value}"))}]").AsWarning())
                .Run();
        }

        if (app.Params.Count > 0)
        {
            LogBuilder.Message(" Params: ")
                .NoNewLineAfter()
                .WaitFor(() => LogBuilder.Message($"[{string.Join(", ", app.Params.Select(p => $"{p.Key}:{p.Value}"))}]").AsSuccess())
                .Run();
        }

        LogMessage.Separator();
    }

    private void DescribeStack(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackArgument>(parseResult, env);

        LogMessage.Separator();

        LogBuilder.Message("Stack: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{stack.Name}").AsSuccess())
            .Run();

        LogBuilder.Message(" Namespace: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{stack.Namespace}").AsWarning())
            .Run();

        LogBuilder.Message(" Auto Sync: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{stack.EnableAutoSync}").AsSuccess())
            .Run();

        LogBuilder.Message(" Apps: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"[{string.Join(", ", stack.Apps.Select(a => a.Name))}]").AsSuccess())
            .Run();

        LogBuilder.Message(" Images: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"[{string.Join(", ", stack.Images.Select(i => i.Name))}]").AsSuccess())
            .Run();

        LogBuilder.Message(" Volumes: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"[{string.Join(", ", stack.Volumes.Select(v => v.Name))}]").AsSuccess())
            .Run();

        LogBuilder.Message(" Ingresses: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"[{string.Join(", ", stack.Ingresses.Select(i => i.Hostname))}]").AsSuccess())
            .Run();

        LogBuilder.Message(" Redirects: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"[{string.Join(", ", stack.Redirects.Select(r => r.Hostname))}]").AsSuccess())
            .Run();

        LogMessage.Separator();
    }

    private async Task DescribeTemplate(ParseResult parseResult)
    {
        var name = GetName<NameArgument>(parseResult);
        var dev = parseResult.GetValue<bool, DevOption>();
        var git = new GitService();
        var apps = await git.GetAppsAsync(dev ? "dev" : "prod");
        var app = apps.FirstOrDefault(x => x.Name == name);

        if (app is null)
        {
            throw new TemplateNotFoundException(name);
        }

        var template = StackTemplate.Load(name);

        LogMessage.Separator();
        
        await LogBuilder.Message("Template: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{template.Name} (port: {template.Port})").AsSuccess())
            .RunAsync();
        
        await LogBuilder.Message(" Required apps: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder
                .Message($"[{string.Join(", ", template.Requirements)}]")
                .AsError())
            .RunAsync();
        
        await LogBuilder.Message(" Required volumes: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder
                .Message($"[{string.Join(", ", template.Volumes)}]")
                .AsWarning())
            .RunAsync();
        
        await LogBuilder.Message(" Required images: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder
                .Message($"[{string.Join(", ", template.Images)}]")
                .AsWarning())
            .RunAsync();
        
        await LogBuilder.Message(" Required params: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder
                .Message($"[{string.Join(", ", template.Params)}]")
                .AsWarning())
            .RunAsync();
        
        await LogBuilder.Message(" Required secrets: (in vault)")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder
                .Message($"[{string.Join(", ", template.Secrets)}]")
                .AsWarning())
            .RunAsync();
        
        LogMessage.Separator();
    }
}