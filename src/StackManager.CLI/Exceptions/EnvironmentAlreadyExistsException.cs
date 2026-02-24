using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Exceptions;

public class EnvironmentAlreadyExistsException(StackEnvironment environment)
    : Exception($"Environment '{environment.Name}' already exists");
