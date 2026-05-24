using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Templates;

/// <summary>
/// Command for describing a specific template.
/// </summary>
public class DescribeTemplateCommand : ResourceDescribeCommand<StackTemplate, NameArgument>
{
    public DescribeTemplateCommand()
        : base("template", "Describe an application template")
    {
    }

    protected override StackTemplate LoadResource(ParseResult parseResult)
    {
        var name = parseResult.GetValue<string, NameArgument>();
        if (name is null)
        {
            throw new TemplateNotFoundException("");
        }
        try
        {
            return StackTemplate.Load(name);
        }
        catch (TemplateNotFoundException)
        {
            throw new TemplateNotFoundException(name);
        }
    }

    protected override void DisplayResource(StackTemplate resource)
    {
        LogMessage.Separator();
        
        LogBuilder.Message("Template: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Name} (port: {resource.Port})").AsSuccess())
            .Run();
        
        LogBuilder.Message(" Required apps: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder
                .Message($"[{string.Join(", ", resource.Requirements)}]")
                .AsError())
            .Run();
        
        LogBuilder.Message(" Required volumes: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder
                .Message($"[{string.Join(", ", resource.Volumes)}]")
                .AsWarning())
            .Run();
        
        LogBuilder.Message(" Required images: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder
                .Message($"[{string.Join(", ", resource.Images)}]")
                .AsWarning())
            .Run();
        
        LogBuilder.Message(" Required params: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder
                .Message($"[{string.Join(", ", resource.Params)}]")
                .AsWarning())
            .Run();
        
        LogBuilder.Message(" Required secrets: (in vault)")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder
                .Message($"[{string.Join(", ", resource.Secrets)}]")
                .AsWarning())
            .Run();
        
        LogMessage.Separator();
    }
}
