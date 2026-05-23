namespace Talaryon.StackManager.Exceptions;

public class StackAlreadyDeletedException(string name)
    : StackManagerException(
        $"Stack '{name}' is already deleted.",
        "Stack",
        name
    )
{
}
