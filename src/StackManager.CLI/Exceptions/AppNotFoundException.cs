namespace Talaryon.StackManager.Exceptions;

public class AppNotFoundException(string? name = null)
    : StackManagerException(
        name is not null ? $"App '{name}' not found." : "App not found.",
        "App",
        name
    )
{
}
