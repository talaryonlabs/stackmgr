using System.CommandLine;
using Talaryon.StackManager;
using Talaryon.StackManager.Commands;

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
    new BuildCommand(),
    new RemoteCommand()
};

// if (!GitService.IsInstalled)
// {
//     HelperMethods.LogError("Git command not found. Please install Git and try again.");
//     return;
// }
//
// if (!GitService.IsRepository)
// {
//     HelperMethods.LogError("Not a git repository.");
//     return;
// }




try
{
    var parseResult = rootCommand.Parse("--help");
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