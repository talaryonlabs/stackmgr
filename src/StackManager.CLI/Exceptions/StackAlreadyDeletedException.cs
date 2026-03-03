namespace Talaryon.StackManager.Exceptions;

public class StackAlreadyDeletedException(string name) : Exception($"Stack '{name}' is already deleted.");