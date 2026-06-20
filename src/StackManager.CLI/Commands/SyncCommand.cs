using System.CommandLine;
using Talaryon.StackManager.Builder;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Commands;

public class SyncCommand : BaseCommand
{
    public SyncCommand() : base("sync", "Sync a stack")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
        Add(new ApplyOption());
        Add(new ForceOption());
    }

    protected override async Task ExecuteAsync()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        var apply = GetValue<bool, ApplyOption>();
        var force = GetValue<bool, ForceOption>();

        var kustomizeService = GetRequiredService<IKustomizeService>()
            .Directory(stack.LocalDirectory);
        var syncService = GetRequiredService<ISyncService>();
        var builder = new StackBuilder(stack);
        
        if (stack.IsDeleted)
        {
            if (force)
            {
                await DeleteStackFromRemote(stack, syncService);
                return;
            }
            
            LogMessage.AsError("Stack '{stack.Name}' is marked for deletion. Use --force (-f) to delete it from remote.");
            return;
        }

        LogMessage.AsInfo($"Syncing stack '{stack.Name}' ...");
        
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