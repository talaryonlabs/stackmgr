// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using stackmgr;
using stackmgr.Arguments;
using stackmgr.Commands;
using stackmgr.Options;

var stackNameArgument = new StackNameArgument();
var appNameArgument = new StackNameArgument();

var devOption = new Option<bool>("--dev");
var templateOption = new TemplateOption { Required = true };

var listStack = new Command("list-stacks", "List stacks");
var newStack = new NewStackCommand();
var deleteStack = new Command("delete-stack", "Delete a stack") { stackNameArgument };
var enableStack = new Command("enable-stack", "Enable a stack") { stackNameArgument };
var disableStack = new Command("disable-stack", "Disable a stack") { stackNameArgument };
var migrateStack = new Command("migrate-stack", "Migrate a stack") { stackNameArgument };

var newApp = new Command("new-app", "Create a new app") { stackNameArgument, appNameArgument };
var addApp = new Command("add-app", "Add an app to a stack") { stackNameArgument, appNameArgument, devOption, templateOption };
var migrateApp = new Command("migrate-app", "Migrate an app to a stack") { stackNameArgument, appNameArgument, templateOption, devOption };
var removeApp = new Command("remove-app", "Remove an app from a stack") { stackNameArgument, appNameArgument };

var rootCommand = new RootCommand
{
    listStack,
    newStack,
    deleteStack,
    enableStack,
    disableStack,
    migrateStack,
    newApp,
    addApp,
    migrateApp,
    removeApp
};

var manager = new StackManager();

manager.RegisterListStacks(listStack);

var parseResult = rootCommand.Parse(args);

if(parseResult.CommandResult.Command == newStack)
{
    var stackName = parseResult.GetRequiredValue(stackNameArgument);
    
    Console.WriteLine($"Creating new stack with name: {stackName}");
}

parseResult.Invoke();

Console.WriteLine("Hello, World!");