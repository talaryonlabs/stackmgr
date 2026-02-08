// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using stackmgr;
using stackmgr.Commands;
using stackmgr.Options;

var rootCommand = new RootCommand
{
    new EnvironmentOption { Required = false, Recursive = true },
    new EnvCommand(),
    new StackCommand(),
    new AppCommand()
};


// if (!StackMgrConfig.Exists && parseResult.CommandResult.Command != initCommand)
// {
//     Console.WriteLine("Stack repository not initialized.");
//     Console.WriteLine("Run `stackmgr init` to initialize the stack repository.");
//     Console.WriteLine();
//     parseResult = rootCommand.Parse("--help");
// }
try
{
    var parseResult = rootCommand.Parse("stack --env test list");
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