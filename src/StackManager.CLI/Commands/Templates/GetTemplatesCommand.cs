using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Templates;

/// <summary>
/// Command for listing available templates.
/// </summary>
public class GetTemplatesCommand : ResourceGetCommand<StackTemplate>
{
    public GetTemplatesCommand()
        : base("templates", "List available application templates", "Templates")
    {
    }

    protected override IReadOnlyList<StackTemplate> GetResources(ParseResult parseResult)
    {
        if (!StackTemplate.AppDirectory.Exists)
        {
            return [];
        }

        var templates = new List<StackTemplate>();
        foreach (var dir in StackTemplate.AppDirectory.GetDirectories())
        {
            try
            {
                var template = StackTemplate.Load(dir.Name);
                templates.Add(template);
            }
            catch
            {
                // Skip directories that don't contain valid templates
            }
        }
        return templates;
    }

    protected override void DisplayResource(StackTemplate resource)
    {
        LogMessage.AsSuccess($"- {resource.Name} (port: {resource.Port})");
    }
}
