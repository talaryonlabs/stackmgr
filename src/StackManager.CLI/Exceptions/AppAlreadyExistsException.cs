using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Exceptions;

public class AppAlreadyExistsException(Stack stack, StackApp app)
    : Exception($"App '{app.Name}' already exists in stack '{stack.Name}' ({stack.Environment.Name})");