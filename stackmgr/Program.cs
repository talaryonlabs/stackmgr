// See https://aka.ms/new-console-template for more information

using System.CommandLine;

Argument<string> stackNameArgument = new Argument<string>("stack-name");
Argument<string> appNameArgument = new Argument<string>("app-name");

Option<bool> devOption = new Option<bool>("--dev");
Option<string> templateOption = new Option<string>("--template") { Required = true };

Command listStack = new Command("list-stacks", "List stacks");
Command newStack = new Command("new-stack", "Create a new stack") { stackNameArgument };
Command deleteStack = new Command("delete-stack", "Delete a stack") { stackNameArgument };
Command enableStack = new Command("enable-stack", "Enable a stack") { stackNameArgument };
Command disableStack = new Command("disable-stack", "Disable a stack") { stackNameArgument };
Command migrateStack = new Command("migrate-stack", "Migrate a stack") { stackNameArgument };

Command newApp = new Command("new-app", "Create a new app") { stackNameArgument, appNameArgument };
Command addApp = new Command("add-app", "Add an app to a stack") { stackNameArgument, appNameArgument, devOption, templateOption };
Command migrateApp = new Command("migrate-app", "Migrate an app to a stack") { stackNameArgument, appNameArgument, templateOption, devOption };
Command removeApp = new Command("remove-app", "Remove an app from a stack") { stackNameArgument, appNameArgument };

RootCommand rootCommand = new RootCommand();
rootCommand.Add(listStack);
rootCommand.Add(newStack);
rootCommand.Add(deleteStack);
rootCommand.Add(enableStack);
rootCommand.Add(disableStack);
rootCommand.Add(migrateStack);
rootCommand.Add(newApp);
rootCommand.Add(addApp);
rootCommand.Add(migrateApp);
rootCommand.Add(removeApp);

listStack.SetAction(async (v, c) =>
{
    return await Task.Run(() =>
    {
        Console.WriteLine("Listing stacks async");
        return 0;
    }, c);
});

ParseResult parseResult = rootCommand.Parse(args);

if(parseResult.CommandResult.Command == newStack)
{
    var stackName = parseResult.GetRequiredValue(stackNameArgument);
    
    Console.WriteLine($"Creating new stack with name: {stackName}");
}

parseResult.Invoke();

Console.WriteLine("Hello, World!");