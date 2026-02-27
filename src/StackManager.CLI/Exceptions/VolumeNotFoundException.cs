namespace Talaryon.StackManager.Exceptions;

public class VolumeNotFoundException(string? name = null) : Exception (name is not null ? $"Volume '{name}' not found." : "Volume not found.");