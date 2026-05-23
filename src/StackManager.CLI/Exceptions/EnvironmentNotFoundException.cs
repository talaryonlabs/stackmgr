namespace Talaryon.StackManager.Exceptions;

public class EnvironmentNotFoundException(string? name = null)
    : StackManagerException(
        name is not null ? $"Environment '{name}' not found." : "Environment not found.",
        "Environment",
        name
    )
{
}
