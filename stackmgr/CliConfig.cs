namespace stackmgr;

public class CliConfig
{
    public const string FileName = ".stackmgr";
    
    private static readonly string FilePath = Path.Combine(Directory.GetCurrentDirectory(), FileName);

    public static bool Exists => new FileInfo(FilePath).Exists;

    public static void Create()
    {
        File.Create(FilePath);
    }
    
    public static CliConfig? LoadConfig()
    {
        var file = new FileInfo(FilePath);
        
        if(!file.Exists) return null;
        return new CliConfig();
    }
}