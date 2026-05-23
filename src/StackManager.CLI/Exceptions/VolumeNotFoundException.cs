namespace Talaryon.StackManager.Exceptions;

public class VolumeNotFoundException(string? name = null)
    : StackManagerException(
        name is not null ? $"Volume '{name}' not found." : "Volume not found.",
        "Volume",
        name
    )
{
}
