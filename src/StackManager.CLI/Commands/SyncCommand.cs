using System.CommandLine;
using Talaryon.StackManager.Builder;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Commands;

public class SyncCommand : BaseCommand
{
    public SyncCommand() : base("sync", "Sync a stack")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
        Add(new ApplyOption());
        SetAction(SyncStack);
    }
    
    private async Task SyncStack(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var apply = parseResult.GetValue<bool, ApplyOption>();

        var kustomizeService = GetRequiredService<IKustomizeService>()
            .Directory(stack.LocalDirectory);
        var syncService = GetRequiredService<ISyncService>();
        var builder = new StackBuilder(stack);
        
        if (stack.IsDeleted)
        {
            LogBuilder.Question("Are you sure you want to delete stack '{stack.Name}' from remote?")
                .AsYesNo()
                .AsWarning()
                .InBox()
                .WaitFor(async result =>
                {
                    if (!result) return LogBuilder.Message("Aborted.");
                    await DeleteStackFromRemote(stack, syncService);
                    return LogBuilder.Message("Done.").AsSuccess();
                });

            return;
        }
        
        await LogBuilder.Message("- [Stack] Building ... ")
            .NoNewLineAfter()
            .WaitFor(() =>
            {
                builder.BuildAll();
                return LogBuilder.Message("Done.").AsSuccess();

            })
            .RunAsync();
        
        var errors = default(List<string>);
        await LogBuilder.Message("- [Stack] Validating ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                if ((errors = await kustomizeService.ValidateAsync()).Count > 0)
                {
                    return LogBuilder.Message("Failed.").AsError();
                }
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .RunAsync();
        
        if(errors is {Count: > 0})
        {
            foreach (var error in errors)
            {
                LogMessage.AsError($"> {error}");
            }

            return;
        }

        await syncService.SyncStackAsync(stack, apply);
    }

    private async Task DeleteStackFromRemote(Stack stack, ISyncService syncService)
    {
        LogMessage.AsInfo($"Deleting stack '{stack.Name}' from remote.");
        if (!await syncService.DeleteStackAsync(stack))
        {
            throw new Exception("Stack deletion not completed. Please try again.");
        }
        stack.Delete(true);
        LogMessage.AsSuccess($"Stack '{stack.Name}' deleted successfully.");
    }
}