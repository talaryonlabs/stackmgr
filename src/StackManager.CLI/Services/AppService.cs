using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Services;

/// <summary>
/// Service for managing app templates and extensions.
/// Base templates are stored in the 'base' subfolder.
/// User implementations/extensions are stored in the 'implementations' subfolder.
/// When building, files in 'implementations' take precedence over 'base'.
/// </summary>
public class AppService
{
    private readonly StackApp _app;
    
    public DirectoryInfo BaseDirectory { get; }
    public DirectoryInfo ImplementationsDirectory { get; }
    
    public AppService(StackApp app)
    {
        _app = app;
        BaseDirectory = new DirectoryInfo(Path.Combine(app.LocalDirectory.FullName, "base"));
        ImplementationsDirectory = new DirectoryInfo(Path.Combine(app.LocalDirectory.FullName, "implementations"));
    }
    
    public void Initialize()
    {
        if (!BaseDirectory.Exists)
        {
            BaseDirectory.Create();
        }
        
        if (!ImplementationsDirectory.Exists)
        {
            ImplementationsDirectory.Create();
        }
    }
    
    public Dictionary<string, FileInfo> GetBuildFiles()
    {
        var files = new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);
        
        if (BaseDirectory.Exists)
        {
            foreach (var file in BaseDirectory.GetFiles("*", SearchOption.AllDirectories))
            {
                var relativePath = GetRelativePath(file, BaseDirectory);
                files[relativePath] = file;
            }
        }
        
        if (ImplementationsDirectory.Exists)
        {
            foreach (var file in ImplementationsDirectory.GetFiles("*", SearchOption.AllDirectories))
            {
                var relativePath = GetRelativePath(file, ImplementationsDirectory);
                files[relativePath] = file;
            }
        }
        
        return files;
    }
    
    private static string GetRelativePath(FileInfo file, DirectoryInfo root)
    {
        var fullPath = file.FullName;
        var rootPath = root.FullName;
        
        if (fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            var relative = fullPath.Substring(rootPath.Length);
            return relative.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        
        return file.Name;
    }
    
    public async Task CopyTemplateToBaseAsync(StackTemplate template)
    {
        if (!template.LocalDirectory.Exists)
        {
            throw new DirectoryNotFoundException(
                $"Template directory '{template.LocalDirectory.FullName}' not found.");
        }
        
        Initialize();
        
        foreach (var file in template.LocalDirectory.GetFiles("*", SearchOption.AllDirectories))
        {
            if (file.Name.Equals(StackTemplate.FileName, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }
            
            var relativePath = GetRelativePath(file, template.LocalDirectory);
            var destPath = Path.Combine(BaseDirectory.FullName, relativePath);
            var destFile = new FileInfo(destPath);
            
            if (!destFile.Directory!.Exists)
            {
                destFile.Directory.Create();
            }
            
            var content = await File.ReadAllTextAsync(file.FullName);
            await File.WriteAllTextAsync(destFile.FullName, content);
        }
    }
    
    public async Task BuildAsync(DirectoryInfo outputDirectory)
    {
        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }
        
        var files = GetBuildFiles();
        
        foreach (var (relativePath, file) in files)
        {
            var destPath = Path.Combine(outputDirectory.FullName, relativePath);
            var destFile = new FileInfo(destPath);
            
            if (!destFile.Directory!.Exists)
            {
                destFile.Directory.Create();
            }
            
            var content = await File.ReadAllTextAsync(file.FullName);
            await File.WriteAllTextAsync(destFile.FullName, content);
        }
    }
    
    public bool HasImplementation(string relativePath)
    {
        var filePath = Path.Combine(ImplementationsDirectory.FullName, relativePath);
        return File.Exists(filePath);
    }
    
    public FileInfo? GetEffectiveFile(string relativePath)
    {
        var implPath = Path.Combine(ImplementationsDirectory.FullName, relativePath);
        if (File.Exists(implPath))
        {
            return new FileInfo(implPath);
        }
        
        var basePath = Path.Combine(BaseDirectory.FullName, relativePath);
        if (File.Exists(basePath))
        {
            return new FileInfo(basePath);
        }
        
        return null;
    }
}
