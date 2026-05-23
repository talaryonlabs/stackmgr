using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Exceptions;

public class StackAlreadyExistsException(Stack stack)
    : StackManagerException(
        $"Stack '{stack.Name}' already exists in environment '{stack.Environment.Name}'",
        "Stack",
        stack.Name,
        null,
        stack.Environment.Name
    )
{
}
