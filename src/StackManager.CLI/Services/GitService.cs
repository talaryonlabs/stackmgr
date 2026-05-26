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
    Task AddIgnoreEntriesAsync(string[] entries);
}



public class GitService : IGitService, IGitServiceActions
{
    private DirectoryInfo _currentDirectory = new(Environment.CurrentDirectory);

    bool IGitServiceActions.IsRepository => _currentDirectory.GetDirectory(".git").Exists;
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
        _currentDirectory = _currentDirectory.GetDirectory(directory);
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
        if(messages.Length == 0)
            throw new ArgumentException("At least one message is required.");
        
        messages = messages
            .Select(v => v.StartsWith("\"") ? v : $"\"{v}\"")
            .ToArray();
        
        var message = messages.Length == 1 ? messages[0] : string.Join(" -m ", messages);
        var process = StartProcess($"commit -m {message}");
        if (process is null)
            throw new SystemErrorException("Git commit failed.");
        
        await process.WaitForExitAsync();
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
        
        if(_currentDirectory.GetDirectory(".git").Exists)
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

    async Task IGitServiceActions.AddIgnoreEntriesAsync(string[] entries)
    {
        var gitignore = _currentDirectory.GetFile(".gitignore");
        if (!gitignore.Exists)
            gitignore.Create();

        var lines = (await File.ReadAllLinesAsync(gitignore.FullName)).ToList();
        var modified = false;
        foreach (var entry in entries)
        {
            if (!lines.Any(v => v.Trim().Equals(entry)))
            {
                lines.Add(entry);
                modified = true;
            }
        }
        if (modified)
            await File.WriteAllLinesAsync(gitignore.FullName, lines);
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