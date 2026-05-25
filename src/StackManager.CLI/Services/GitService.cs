using System.Diagnostics;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Services;

public interface IGitService
{
    bool IsInstalled { get; }
    IGitServiceActions CurrentDirectory();
    IGitServiceActions Directory(DirectoryInfo directory);
    IGitServiceActions Directory(string directory);
}

public interface IGitServiceActions
{
    bool IsRepository { get; }
    Task<string> DiffAsync();
    Task<string> DiffAsync(FileInfo file1, FileInfo file2);
    Task PullAsync();
    Task CommitAsync(string message);
    Task CommitAsync(string[] messages);
    Task PushAsync();
    Task ResetAsync();
    Task AddAsync();
    Task CloneAsync(string url);
    Task CheckoutAsync(string branch);
}



public class GitService : IGitService, IGitServiceActions
{
    private DirectoryInfo _currentDirectory = new(Environment.CurrentDirectory);

    bool IGitServiceActions.IsRepository => Directory.Exists(Path.Combine(_currentDirectory.FullName, ".git"));
    bool IGitService.IsInstalled
    {
        get
        {
            try
            {
                StartProcess("--version")!.WaitForExit();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }

    IGitServiceActions IGitService.CurrentDirectory()
    {
        _currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);
        return this;
    }

    IGitServiceActions IGitService.Directory(DirectoryInfo directory)
    {
        _currentDirectory = directory;
        return this;
    }
    
    IGitServiceActions IGitService.Directory(string directory)
    {
        _currentDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, directory));
        return this;
    }

    async Task IGitServiceActions.PullAsync()
    {
        var process = StartProcess("pull -q");
        if(process is null)
            throw new SystemErrorException("Git pull failed.");
        
        await process.WaitForExitAsync();
    }

    async Task<string> IGitServiceActions.DiffAsync()
    {
        var process = StartProcess("diff --name-only");
        if (process is null)
            throw new SystemErrorException("Git diff failed.");
        
        await process.WaitForExitAsync();
        return await process.StandardOutput.ReadToEndAsync();
    }

    async Task<string> IGitServiceActions.DiffAsync(FileInfo file1, FileInfo file2)
    {
        var process = StartProcess($"diff --no-index \"{file1.FullName}\" \"{file2.FullName}\"");
        if (process is null)
            throw new SystemErrorException("Git diff failed.");
        
        await process.WaitForExitAsync();
        return await process.StandardOutput.ReadToEndAsync();
    }

    async Task IGitServiceActions.CommitAsync(string message)
    {
        var process = StartProcess($"commit -m \"{message}\"");
        if (process is null)
            throw new SystemErrorException("Git commit failed.");
        
        await process.WaitForExitAsync();
    }
    
    async Task IGitServiceActions.CommitAsync(string[] messages)
    {
        var message = string.Join(" -m ", messages);
        await (this as IGitServiceActions).CommitAsync(message);
    }
    
    async Task IGitServiceActions.PushAsync()
    {
        var process = StartProcess("push -q");
        if (process is null)
            throw new SystemErrorException("Git push failed.");
        
        await process.WaitForExitAsync();
    }
    
    async Task IGitServiceActions.ResetAsync()
    {
        var process = StartProcess("reset -q");
        if (process is null)
            throw new SystemErrorException("Git reset failed.");
        
        await process.WaitForExitAsync();
    }
    
    async Task IGitServiceActions.AddAsync()
    {
        var process = StartProcess("add .");
        if (process is null)
            throw new SystemErrorException("Git add failed.");
        
        await process.WaitForExitAsync();
    }

    async Task IGitServiceActions.CloneAsync(string url)
    {
        if(!_currentDirectory.Exists)
            _currentDirectory.Create();
        
        if(Directory.Exists(Path.Combine(_currentDirectory.FullName, ".git")))
            throw new SystemErrorException("Directory already has a .git directory.");
        
        var process = StartProcess($"clone {url}");
        if (process is null)
            throw new SystemErrorException("Git clone failed.");
        
        await process.WaitForExitAsync();
    }

    async Task IGitServiceActions.CheckoutAsync(string branch)
    {
        var process = StartProcess($"checkout {branch} -q");
        if (process is null)
            throw new SystemErrorException("Git checkout failed.");
        
        await process.WaitForExitAsync();
    }


    private Process? StartProcess(string command)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "git",
            Arguments = command,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _currentDirectory.FullName
        });

        return process;
    }
}