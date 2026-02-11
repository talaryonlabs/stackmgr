using System.Diagnostics;

namespace stackmgr.Services;

public class GitService
{
    public static bool IsRepository => Directory.Exists(Path.Combine(Environment.CurrentDirectory, ".git"));
    public static bool IsInstalled
    {
        get
        {
            try
            {
                Process
                    .Start(new ProcessStartInfo("git", "--version")
                    {
                        RedirectStandardOutput = true
                    })
                    ?.WaitForExit();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }

    private readonly StackEnvironment _env;
    
    public GitService(StackEnvironment env)
    {
        _env = env;
        if (_env.AppRepository is null)
            throw new Exception("App repository cannot be null");
        
    }

    private void ApplyIgnoreFile()
    {
        var items = new List<string> { ".apps", ".stackmgr" };
        var path = Path.Combine(Environment.CurrentDirectory, ".gitignore");
        var file = new FileInfo(path);
        if (!file.Exists) file.Create().Close();

        var lines = File.ReadAllLines(file.FullName).ToList();
        foreach (var item in items.Where(item => !lines.Contains(item)))
        {
            lines.Add(item);
        }
        File.WriteAllLines(file.FullName, lines);
    }

    public async Task<DirectoryInfo[]> GetAppsAsync(string branch)
    {
        ApplyIgnoreFile();
        
        var apps = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, ".apps"));
        
        if (!apps.Exists || apps.GetDirectories(".git").Length == 0)
        {
            var clone = Process.Start(new ProcessStartInfo("git",
                $"clone -v {_env.AppRepository} {apps.FullName}")
            {
                RedirectStandardOutput = true
            });
            if(clone is not null) await clone.WaitForExitAsync();
        }
        else
        {
            var pull = Process.Start(new ProcessStartInfo("git", "pull -v")
            {
                WorkingDirectory = apps.FullName,
                RedirectStandardOutput = true
            });
            if(pull is not null) await pull.WaitForExitAsync();
        }
        
        var checkout = Process.Start(new ProcessStartInfo("git", $"checkout {branch}")
        {
            WorkingDirectory = apps.FullName,
            RedirectStandardOutput = true
        });
        if(checkout is not null) await checkout.WaitForExitAsync();
        
        return apps.GetDirectories();
    }
    
    public Task PullAsync()
    {
        ApplyIgnoreFile();
        
        var pull = Process.Start(new ProcessStartInfo("git", "pull -v")
        {
            WorkingDirectory = Environment.CurrentDirectory,
        });
        return pull?.WaitForExitAsync() ?? Task.CompletedTask;
    }
}