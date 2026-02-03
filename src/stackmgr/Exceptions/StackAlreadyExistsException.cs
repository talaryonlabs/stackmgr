namespace stackmgr.Exceptions;

public class StackAlreadyExistsException(Stack stack)
    : Exception($"Stack '{stack.Name}' already exists in environment '{stack.Environment.Name}'");