namespace Talaryon.StackManager.Exceptions;

public class ResourceAlreadyExistsException<T>(Stack stack, string name)
    : StackManagerException(
        $"{typeof(T).Name} '{name}' already exists in stack '{stack.Name}' ({stack.Environment.Name})",
        typeof(T).Name,
        name,
        stack.Name,
        stack.Environment.Name
    )
    where T : class, IStackObject
{
}