using System.Diagnostics;

namespace stackmgr.Services;

public static class Git
{
    private static DirectoryInfo Apps => new(Path.Combine(Environment.CurrentDirectory, ".apps"));
    public static bool IsRepository => Directory.Exists(Path.Combine(Environment.CurrentDirectory, ".git"));
    public static bool IsInstalled
    {
        get
        {
            try
            {
                Process
                    .Start("git", "--version")
                    .WaitForExit();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }

    public static void ApplyIgnoreFile()
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

    public static async Task GetApps()
    {
        if (!Apps.Exists || Apps.GetDirectories(".git").Length == 0)
        {
            await Process
                .Start("git",
                    $"clone -v https://github.com/talaryonlabs/apps {Apps.FullName}")
                .WaitForExitAsync();

            return;
        }

        var pull = Process.Start(new ProcessStartInfo("git", "pull -v")
        {
            WorkingDirectory = Apps.FullName
        });
        if(pull is not null) await pull.WaitForExitAsync();
    }

    public static async Task CheckoutApps(string branch)
    {
        if (!Apps.Exists) throw new Exception("Apps directory not found.");
        
        var checkout = Process.Start(new ProcessStartInfo("git", $"checkout {branch}")
        {
            WorkingDirectory = Apps.FullName
        });
        if(checkout is not null) await checkout.WaitForExitAsync();
    }

    public static Task Pull() =>
        Process
            .Start("git", "pull")
            .WaitForExitAsync();
    
    public static Task Fetch() =>
        Process
            .Start("git", "fetch")
            .WaitForExitAsync();
}