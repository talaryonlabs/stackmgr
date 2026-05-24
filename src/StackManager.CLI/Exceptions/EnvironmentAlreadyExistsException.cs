namespace Talaryon.StackManager.Exceptions;

public class EnvironmentAlreadyExistsException(StackEnvironment environment)
    : StackManagerException(
        $"Environment '{environment.Name}' already exists",
        "Environment",
        environment.Name
    )
{
}
