// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using stackmgr;
using stackmgr.Commands;


var initCommand = new InitCommand();

var rootCommand = new RootCommand
{
    initCommand,
    new StackCommand()
};

var manager = new StackManager();

var parseResult = rootCommand.Parse(args);
if (!CliConfig.Exists && parseResult.CommandResult.Command != initCommand)
{
    Console.WriteLine("Stack repository not initialized.");
    Console.WriteLine("Run `stackmgr init` to initialize the stack repository.");
    Console.WriteLine();
    parseResult = rootCommand.Parse("--help");
}
parseResult.Invoke();