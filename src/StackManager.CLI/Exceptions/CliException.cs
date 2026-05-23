using System;

namespace Talaryon.StackManager.Exceptions;

/// <summary>
/// Base exception for CLI errors with exit code support
/// </summary>
public class CliException : Exception
{
    /// <summary>
    /// Exit code to return (0 = success, 1 = user error, 2 = system error)
    /// </summary>
    public int ExitCode { get; }
    
    /// <summary>
    /// Whether to show stack trace in error output
    /// </summary>
    public bool ShowStackTrace { get; set; } = false;
    
    public CliException(string message, int exitCode = 1, Exception? innerException = null)
        : base(message, innerException)
    {
        ExitCode = exitCode;
    }
}

/// <summary>
/// User error (invalid input, missing arguments, etc.) - Exit code 1
/// </summary>
public class UserErrorException : CliException
{
    public UserErrorException(string message, Exception? innerException = null)
        : base(message, 1, innerException) { }
}

/// <summary>
/// System/Infrastructure error (API failures, file system, etc.) - Exit code 2
/// </summary>
public class SystemErrorException : CliException
{
    public SystemErrorException(string message, Exception? innerException = null)
        : base(message, 2, innerException) { }
}

/// <summary>
/// Configuration error (missing config, invalid settings) - Exit code 3
/// </summary>
public class ConfigurationException : CliException
{
    public ConfigurationException(string message, Exception? innerException = null)
        : base(message, 3, innerException) { }
}
