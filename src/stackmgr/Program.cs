// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using stackmgr;
using stackmgr.Commands;
using stackmgr.Services;

var rootCommand = new RootCommand
{
    new NewCommand(),
    new TestCommand(),
    new GetCommand(),
    new DeleteCommand(),
    new ConfigureCommand(),
    new SyncCommand(),
    new DefaultCommand(),
    new MigrateCommand(),
    new BuildCommand()
};

// DEV
Directory.SetCurrentDirectory(@"D:\Developing\stack");
Environment.CurrentDirectory = Directory.GetCurrentDirectory();

if (!Git.IsInstalled)
{
    HelperMethods.LogError("Git command not found. Please install Git and try again.");
    return;
}

if (!Git.IsRepository)
{
    HelperMethods.LogError("Not a git repository.");
    return;
}

Git.ApplyIgnoreFile();
// await Git.GetApps();
// await Git.CheckoutApps("dev");

// await Git.Pull();

// if (!StackMgrConfig.Exists && parseResult.CommandResult.Command != initCommand)
// {
//     Console.WriteLine("Stack repository not initialized.");
//     Console.WriteLine("Run `stackmgr init` to initialize the stack repository.");
//     Console.WriteLine();
//     parseResult = rootCommand.Parse("--help");
// }
try
{
    var parseResult = rootCommand.Parse("""migrate app talaryonlabs test --without-ingress""");
    if (args.Length > 0)
    { 
        parseResult = rootCommand.Parse(args);    
    }
    await parseResult.InvokeAsync(new InvocationConfiguration()
    {
        EnableDefaultExceptionHandler = false
        
    });
}
catch (Exception ex)
{
    HelperMethods.LogError(ex.Message);
}