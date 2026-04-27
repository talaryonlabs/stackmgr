using System.Security.Cryptography;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Services;

public class AppMigrator(StackApp app)
{
    private readonly StackApp _app = app;
    private readonly GitService _git = new();
    private bool _fetched;
    private bool _preflight;

    public AppMigratorPreflight? PreflightResult { get; private set; }

    public async Task Fetch()
    {
        if (_app.Template is null) throw new InvalidOperationException($"App '{_app.Name}' has no template.");
        
        await _git.GetAppsAsync(_app.Template.Branch);
        _fetched = true;
    }

    public async Task<bool> Preflight()
    {
        if (_app.Template is null) throw new InvalidOperationException($"App '{_app.Name}' has no template.");
        if (!_fetched) throw new InvalidOperationException("Fetch must be called before Preflight.");

        var template = StackTemplate.Load(_app.Template.Name);
        PreflightResult = new AppMigratorPreflight();
        var templateDir = template.LocalDirectory;
        var appDir = _app.LocalDirectory;

        if (!templateDir.Exists)
        {
            throw new DirectoryNotFoundException($"Template directory '{templateDir.FullName}' not found.");
        }

        var templateFiles = templateDir.GetFiles("*", SearchOption.AllDirectories)
            .Where(f => !f.Name.Equals(StackTemplate.FileName, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        foreach (var templateFile in templateFiles)
        {
            var relativePath = templateFile.FullName.Substring(templateDir.FullName.Length + 1);
            var destFile = new FileInfo(Path.Combine(appDir.FullName, relativePath));
            var preflightFile = new AppMigratorPreflightFile
            {
                Source = templateFile,
                Destination = destFile
            };

            if (!destFile.Exists)
            {
                PreflightResult.Adding.Add(preflightFile);
            }
            else if (FilesDiffer(templateFile, destFile))
            {
                preflightFile.Difference = await _git.DiffAsync(templateFile, destFile);
                PreflightResult.Migrating.Add(preflightFile);
            }
        }

        _preflight = true;
        return PreflightResult.Adding.Count > 0 || PreflightResult.Migrating.Count > 0;
    }

    private static bool FilesDiffer(FileInfo file1, FileInfo file2)
    {
        using var md5 = MD5.Create();
        
        var hash1 = ComputeHash(md5, file1);
        var hash2 = ComputeHash(md5, file2);
        
        return !hash1.SequenceEqual(hash2);
    }

    private static byte[] ComputeHash(HashAlgorithm hashAlgorithm, FileInfo file)
    {
        using var stream = file.OpenRead();
        return hashAlgorithm.ComputeHash(stream);
    }

    public async Task Migrate()
    {
        if (_app.Template is null) throw new InvalidOperationException($"App '{_app.Name}' has no template.");
        if (!_preflight) throw new InvalidOperationException("Preflight must be called before Migrate.");
    }
}

public class AppMigratorPreflight
{
    public List<AppMigratorPreflightFile> Migrating { get; } = new();
    public List<AppMigratorPreflightFile> Adding { get; } = new();
}

public class AppMigratorPreflightFile
{
    public FileInfo? Source { get; set; }
    public FileInfo? Destination { get; set; }
    public string Difference { get; set; } = string.Empty;
}
