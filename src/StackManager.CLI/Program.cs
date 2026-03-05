using System.CommandLine;
using Talaryon.StackManager;
using Talaryon.StackManager.Commands;
using Talaryon.StackManager.Services;
using Talaryon.Toolbox.Api;

var rootCommand = new RootCommand
{
    new NewCommand(),
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
catch (ApiError error)
{
    LogMessage.AsError(error.Message ?? "Unknown error");   
}
catch (Exception ex)
{
    LogMessage.AsError(ex.Message);
}