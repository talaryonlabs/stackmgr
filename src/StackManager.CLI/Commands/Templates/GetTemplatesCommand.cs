using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Commands.Templates;

/// <summary>
/// Command for listing available templates.
/// </summary>
public class GetTemplatesCommand : ResourceGetCommand<StackTemplate>
{
    /// <summary>
    /// Command for listing available templates.
    /// </summary>
    public GetTemplatesCommand() : base("templates", "List available application templates", "Templates")
    {
        Add(new DevOption());
    }

    protected override async Task<IReadOnlyList<StackTemplate>> GetResourcesAsync(ParseResult parseResult)
    {
        var templateService = GetRequiredService<ITemplateService>();
        var isDev = parseResult.GetValue<bool, DevOption>();

        await templateService.UpdateAsync(isDev ? "dev" : "prod");
        
        return templateService.GetTemplates();
    }

    protected override void DisplayResource(StackTemplate resource)
    {
        LogMessage.AsSuccess($"- {resource.Name} (port: {resource.Port})");
    }
}
