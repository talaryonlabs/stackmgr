namespace Talaryon.StackManager.Exceptions;

public class EnvironmentAlreadyExistsException(string name)
    : StackManagerException(
        $"Environment '{name}' already exists",
        "Environment",
        name
    )
{
}
