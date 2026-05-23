namespace Talaryon.StackManager.Exceptions;

public class TemplateNotFoundException(string? name = null)
    : StackManagerException(
        name is not null ? $"Template '{name}' not found." : "Template not found.",
        "Template",
        name
    )
{
}
