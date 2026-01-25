using System.CommandLine;
using stackmgr.Options;

namespace stackmgr.Commands;

public class InitCommand : Command
{
    public InitCommand() : base("init", "Initialize stack repository")
    {
        var gitignore = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), ".gitignore"));
        var environments = Enum.GetValues<StackEnvironment>();
        
        SetAction(_ =>
        {
            if(!gitignore.Exists) gitignore
                .Create()
                .Close();
            
            var lines = File.ReadAllLines(gitignore.FullName);
            if (!lines.Contains(".stackmgr")) lines = lines.Append(".stackmgr").ToArray();
            File.WriteAllLines(gitignore.FullName, lines);
            
            if (StackMgrConfig.Exists)
            {
                Console.WriteLine($"{StackMgrConfig.FileName} already exists. Nothing to do.");
            }
            else
            {
                foreach (var env in environments)
                {
                    var envPath = Path.Combine(Directory.GetCurrentDirectory(), env.ToString().ToLower());
                    if (!Directory.Exists(envPath))
                    {
                        Directory.CreateDirectory(envPath);
                        Console.WriteLine($"Created directory {envPath}");
                    }
                }
                StackMgrConfig.Create();
            }
        });
    }
}