namespace Talaryon.StackManager.Exceptions;

public class ImageNotFoundException(string? name = null) : Exception (name is not null ? $"Image '{name}' not found." : "Image not found.");