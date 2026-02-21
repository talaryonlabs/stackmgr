using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Exceptions;

public class StackAlreadyExistsException(Stack stack)
    : Exception($"Stack '{stack.Name}' already exists in environment '{stack.Environment.Name}'");