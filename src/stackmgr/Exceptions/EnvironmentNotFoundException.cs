namespace stackmgr.Exceptions;

public class EnvironmentNotFoundException(string? name = null) : Exception (name is not null ? $"Environment '{name}' not found." : "Environment not found.");