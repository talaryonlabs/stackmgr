namespace Talaryon.StackManager.Exceptions;

public class IngressNotFoundException(string? name = null)
    : StackManagerException(
        name is not null ? $"Ingress '{name}' not found." : "Ingress not found.",
        "Ingress",
        name
    )
{
}
