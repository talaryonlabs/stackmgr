namespace Talaryon.StackManager.Exceptions;

public class StackNotFoundException(string? name = null) : Exception (name is not null ? $"Stack '{name}' not found." : "Stack not found.");