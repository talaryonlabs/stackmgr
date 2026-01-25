// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using stackmgr.Commands;

var rootCommand = new RootCommand
{
    new EnvCommand(),
    new StackCommand()
};

var parseResult = rootCommand.Parse(args);
// if (!StackMgrConfig.Exists && parseResult.CommandResult.Command != initCommand)
// {
//     Console.WriteLine("Stack repository not initialized.");
//     Console.WriteLine("Run `stackmgr init` to initialize the stack repository.");
//     Console.WriteLine();
//     parseResult = rootCommand.Parse("--help");
// }
parseResult.Invoke();