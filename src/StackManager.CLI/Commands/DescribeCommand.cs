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
            new StackArgument()
        };
        stack.SetAction(DescribeStack);
        
        var template = new StackManagerCommand("template", "")
        {
            new NameArgument(),
            new DevOption()
        };
        template.SetAction(DescribeTemplate);
        
        Add(env);
        Add(stack);
        Add(template);
    }

    private void DescribeEnvironment(ParseResult parseResult)
    {
        LogMessage.AsWarning("Not implemented yet.");
    }

    private void DescribeStack(ParseResult parseResult)
    {
        LogMessage.AsWarning("Not implemented yet.");
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

        LogMessage.AsInfo("--------");
        
        await LogBuilder.Message("Template: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message(template.Name).AsSuccess())
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
        
        LogMessage.AsInfo("--------");
    }
}