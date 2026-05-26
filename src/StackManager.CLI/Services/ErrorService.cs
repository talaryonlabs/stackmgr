namespace Talaryon.StackManager.Services;

public interface IErrorService
{
    public Exception? LastException { get; }
    public int ExitCode { get; }
    
    public void LogError(Exception exception);
    public void SetExitCode(int exitCode);   
}

public class ErrorService : IErrorService
{
    public Exception? LastException { get; private set; }
    public int ExitCode { get; private set; }

    public void LogError(Exception exception)
    {
        LastException = exception;
    }

    public void SetExitCode(int exitCode)
    {
        ExitCode = exitCode;
    }
}