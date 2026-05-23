using System.CommandLine;
using System.Reflection;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Services;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands;

public class DescribeCommand : BaseCommand
{
    public DescribeCommand() : base("describe", "Describe a resource (environment, stack, template)")
    {
        // Auto-discover and add all ResourceDescribeCommand<TResource, TArg> implementations
        var describeCommandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.BaseType?.IsGenericType == true 
                && t.BaseType.GetGenericTypeDefinition() == typeof(ResourceDescribeCommand<,>)
                && !t.IsAbstract)
            .ToList();

        foreach (var instance in describeCommandTypes
                     .Select(type => (BaseCommand?)Activator.CreateInstance(type))
                     .OfType<BaseCommand>())
        {
            Add(instance);
        }

        // Add template describe separately (doesn't use base class)
        var template = new BaseCommand("template", "Describe an application template")
        {
            new NameArgument(),
            new DevOption()
        };
        template.SetAction(DescribeTemplate);
        Add(template);
    }

    private async Task DescribeTemplate(ParseResult parseResult)
    {
        var name = GetName<NameArgument>(parseResult);
        var dev = parseResult.GetValue<bool, DevOption>();
        var git = GetRequiredService<GitService>();
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
