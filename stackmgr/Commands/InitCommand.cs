using System.CommandLine;
using stackmgr.Options;

namespace stackmgr.Commands;

public class InitCommand : Command
{
    public InitCommand() : base("init", "Initialize stack repository")
    {
        
        var environments = Enum.GetValues<StackEnvironment>();
        
        SetAction(_ =>
        {
            if (CliConfig.Exists)
            {
                Console.WriteLine($"{CliConfig.FileName} already exists. Nothing to do.");
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
                CliConfig.Create();
            }
        });
    }
}