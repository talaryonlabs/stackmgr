namespace Talaryon.StackManager.Exceptions;

public class AppAlreadyExistsException(Stack stack, StackApp app)
    : StackManagerException(
        $"App '{app.Name}' already exists in stack '{stack.Name}' ({stack.Environment.Name})",
        "App",
        app.Name,
        stack.Name,
        stack.Environment.Name
    )
{
}
