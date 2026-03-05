using System.Diagnostics;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Services;

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
    private readonly string _appRepository;
    
    public GitService(StackEnvironment env)
    {
        _env = env;
        _appRepository = LocalConfig.Get().AppRepository;

        if (_appRepository is not { Length: > 0 })
            throw new Exception("App repository cannot be null. Please check your configuration.");

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
                $"clone -q {_appRepository} {apps.FullName}")
            {
                RedirectStandardOutput = true
            });
            if(clone is not null) await clone.WaitForExitAsync();
        }
        else
        {
            var pull = Process.Start(new ProcessStartInfo("git", "pull -q")
            {
                WorkingDirectory = apps.FullName,
            });
            if (pull is not null)
            {
                LogMessage.AsInfo($"Pulling {branch}.");
                await pull.WaitForExitAsync();
            }
        }
        
        var checkout = Process.Start(new ProcessStartInfo("git", $"checkout {branch} -q")
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
        
        var pull = Process.Start(new ProcessStartInfo("git", "pull -q")
        {
            WorkingDirectory = Environment.CurrentDirectory,
        });
        return pull?.WaitForExitAsync() ?? Task.CompletedTask;
    }

    public async Task ApplyAsync(Stack stack)
    {
        ApplyIgnoreFile();
        FileInfo[] files = [];
        
        await PullAsync();
        await LogBuilder.Message("- [Git] Review changes ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                var reset = Process.Start(new ProcessStartInfo("git", "reset -q")
                {
                    WorkingDirectory = Environment.CurrentDirectory,
                });
                if (reset is not null) 
                    await reset.WaitForExitAsync();
                
                var diff = Process.Start(new ProcessStartInfo("git", "diff --name-only")
                {
                    WorkingDirectory = Environment.CurrentDirectory,
                    RedirectStandardOutput = true,
                });
                if(diff is null) throw new Exception("Diff failed.");
                files = (await diff!.StandardOutput.ReadToEndAsync())
                    .Split(Environment.NewLine)
                    .Where(x => x.Length > 0)
                    .Select(x => new FileInfo(x))
                    .ToArray();

                return LogBuilder.Message("Done.").AsSuccess();

            })
            .RunAsync();

        await LogBuilder.Message("- [Git] Commit changes ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                if (files.Length == 0) return LogBuilder.Message("No changes.").AsWarning();
                
                var add = Process.Start(new ProcessStartInfo("git", "add .")
                {
                    WorkingDirectory = Environment.CurrentDirectory,
                });

                if (add is not null) 
                    await add.WaitForExitAsync();
                
                var message = string.Join(" -m ", new[]
                {
                    "\"Apply changes. (StackManager)\"",
                    $"\"> Stack: [{stack.Name}]\"",
                    $"\"> Environment: [{_env.Name}]\"",
                    "\"> Files: \"",
                    string.Join(Environment.NewLine, files.Select(x => $"\" - {x.Name.Trim()}\""))
                });
                
                var commit = Process.Start(new ProcessStartInfo("git", $"commit -m {message} -q")
                {
                    WorkingDirectory = Environment.CurrentDirectory,
                });
                if(commit is not null) 
                    await commit.WaitForExitAsync();
                
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .RunAsync();

        await LogBuilder.Message("- [Git] Push changes ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                var push = Process.Start(new ProcessStartInfo("git", "push -q")
                {
                    WorkingDirectory = Environment.CurrentDirectory,
                });
                if(push is not null) 
                    await push.WaitForExitAsync();
                
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .RunAsync();
    }
}